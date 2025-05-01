using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Yarn.Unity.Example {
	/// <summary>
	/// 运行Yarn命令并管理视觉小说示例中的精灵
	/// </summary>
    public class VNManager : DialogueViewBase
    {
		[SerializeField] public DialogueRunner runner;

		[Header("资源"), Tooltip("如果你不想使用/Resources/文件夹，可以在这里手动分配各种资源")]
		public List<Sprite> loadSprites = new List<Sprite>();
		public List<AudioClip> loadAudio = new List<AudioClip>();

		[Tooltip("如果启用：将自动加载/Resources/文件夹及其所有子文件夹中的所有精灵和音频片段")]
		public bool useResourcesFolders = false;


		[Header("精灵UI设置")] // UI调优变量和引用
		[Tooltip("所有精灵将使用此颜色进行着色")] 
		public Color defaultTint;
		[Tooltip("当说话时，精灵将通过使用此颜色着色来高亮显示")]
		public Color highlightTint;


		[Header("对象引用"), Tooltip("除非你知道自己在做什么，否则不要更改这些")]
		public RectTransform spriteGroup; // 用于屏幕震动
		public Image bgImage, fadeBG, nameplateBG;
		public Image genericSprite; // 本地预制体，用于实例化精灵
		public AudioSource genericAudioSource; // 本地预制体，用于实例化声音
		public RectTransform dialogPanel; // 用于对话面板震动

		// 用于跟踪所有实例化对象的大列表
		List<AudioSource> sounds = new List<AudioSource>(); // 所有实例化声音的大列表
		List<Image> sprites = new List<Image>(); // 所有实例化精灵的大列表

		// 存储"演员"（角色等）的精灵引用
		[HideInInspector] public Dictionary<string, VNActor> actors = new Dictionary<string, VNActor>(); // 跟踪名称到精灵的映射

		static Vector2 screenSize = new Vector2( 1280f, 720f); // 用于位置计算，例如"左侧"是什么意思？

		void Awake () {
			// 手动添加所有Yarn命令处理器，这样我们就不必
			// 在Yarn脚本中键入游戏对象名称（也
			// 通过避免GameObject.Find给我们带来性能提升）
			runner.AddCommandHandler<string>("Scene", DoSceneChange );
			runner.AddCommandHandler<string,string,string,string,string>("Act", SetActor );
			runner.AddCommandHandler<string,string,string>("Draw", SetSpriteYarn );

			runner.AddCommandHandler<string>("Hide", HideSprite );
			runner.AddCommandHandler("HideAll", HideAllSprites );
			runner.AddCommandHandler("Reset", ResetScene );

			runner.AddCommandHandler<string,string,string,float>("Move", MoveSprite );
			runner.AddCommandHandler<string,string>("Flip", FlipSprite );
			runner.AddCommandHandler<string,float>("Shake", ShakeSprite );
			runner.AddCommandHandler<float>("ShakeCamera", ShakeCamera );

			runner.AddCommandHandler<string,float,string>("PlayAudio", PlayAudio );
			runner.AddCommandHandler<string>("StopAudio", StopAudio );
			runner.AddCommandHandler("StopAudioAll", StopAudioAll );

            runner.AddCommandHandler<string,float,float,float>("Fade", SetFade );
			runner.AddCommandHandler<float>("FadeIn", SetFadeIn );
			runner.AddCommandHandler<string,string,float>("CamOffset", SetCameraOffset );

			// 将所有资源添加到内部列表/一个大堆中...
			// 它也会扫描所有子文件夹！注意：但在
			// Yarn脚本中引用精灵时，只需使用文件名
			// 并省略文件夹名称
			if ( useResourcesFolders ) {
				var allSpritesInResources = Resources.LoadAll<Sprite>("");
				loadSprites.AddRange( allSpritesInResources );
				var allAudioInResources = Resources.LoadAll<AudioClip>("");
				loadAudio.AddRange( allAudioInResources );
			}
		}

		#region YarnCommands

		/// <summary>更改背景图像</summary>
		public void DoSceneChange(string spriteName) {
			bgImage.sprite = FetchAsset<Sprite>( spriteName );
		}

		/// <summary>
		/// SetActor(actorName,spriteName,positionX,positionY,color) 主要的
		/// 用于移动/调整角色的函数</summary>
		public void SetActor(string actorName, string spriteName = "", string positionX = "", string positionY = "", string colorHex = "" ) {

			// 如果提供了精灵名称，则创建精灵
			Image newActor = null;
			if (!string.IsNullOrEmpty(spriteName)) {
				// 必须使用SetSprite()因为par[2]和par[3]可能是
				// 关键字（例如"left"，"right"）
				newActor = SetSpriteUnity( spriteName, positionX, positionY );
			}

			// 定义文本标签背景颜色
            var actorColor = Color.black;
			if (colorHex != string.Empty && ColorUtility.TryParseHtmlString( colorHex, out actorColor ) ==false ) {
				Debug.LogErrorFormat(this, "VN管理器无法将[{0}]解析为HTML颜色（例如[#FFFFFF]或某些关键字如[white]）", colorHex);
			}

			// 如果演员已经在使用精灵，则克隆任何
			// 持久数据，并销毁它（只是为了安全起见）
			if ( actors.ContainsKey(actorName)) {
				// 如果缺少任何位置参数，假设演员
				// 位置应该保持不变
				if (newActor != null) {
					var newPos = newActor.rectTransform.anchoredPosition;
					if ( positionX == string.Empty && positionY == string.Empty ) { // 缺少2个参数，覆盖x和y
						newPos = actors[actorName].rectTransform.anchoredPosition;
					} else if ( positionY == string.Empty ) { // 缺少1个参数，覆盖y
						newPos.y = actors[actorName].rectTransform.anchoredPosition.y;
					}
					newActor.rectTransform.anchoredPosition = newPos;
				}
				// 如果缺少任何颜色参数，则假设演员颜色
				// 应该保持不变
				if ( colorHex == string.Empty ) {
					actorColor = actors[actorName].actorColor;
				}
				// 清理
				Destroy( actors[actorName].gameObject );
				actors.Remove(actorName);
			}

			// 保存演员数据
			actors.Add( actorName, new VNActor( newActor, actorColor) );
		}

		///<summary> Draw(spriteName,positionX,positionY) 用于精灵绘制的
		///通用函数</summary>
		public void SetSpriteYarn(string spriteName, string positionX = "", string positionY = "") {
			SetSpriteUnity( spriteName, positionX, positionY );
		}

		public Image SetSpriteUnity(string spriteName, string positionX = "", string positionY = "") {
			
			// 定位精灵
			var pos = new Vector2(0.5f, 0.5f);

            if (positionX != string.Empty) {
                pos.x = ConvertCoordinates(positionX);
            }
            
            if (positionY != string.Empty) {
                pos.y = ConvertCoordinates(positionY);
            }
        
			// 现在实际实例化并绘制精灵
			return SetSpriteActual( spriteName, pos );
		}

		///<summary>Hide(spriteName). "spriteName"可以使用通配符，例如
		///HideSprite(Sally*)将隐藏SallyIdle和
		///Sally_Happy</summary>
		public void HideSprite(string spriteName) {
			
			var wildcard = new Wildcard(spriteName);

			// 生成要删除的内容列表

			var imagesToDestroy = new List<Image>();
			var actorKeysToRemove = new List<string>();
			
			foreach ( var actor in actors ) {
				if ( wildcard.IsMatch(actor.Key) || wildcard.IsMatch(actor.Value.actorImage.name) ) {
					actorKeysToRemove.Add( actor.Key );
					imagesToDestroy.Add(actor.Value.actorImage);
				}
			}

			foreach ( var sprite in sprites ) {
				if ( wildcard.IsMatch(sprite.name) ) {
					imagesToDestroy.Add(sprite);
				}
			}

			// 现在实际删除所有内容，如果有的话

			for( int i=0; i<actorKeysToRemove.Count; i++) {
				if ( actors.ContainsKey( actorKeysToRemove[i] ) ) { // 这应该永远不会为false，但让我们保持安全
					actors.Remove( actorKeysToRemove[i] );
				}
			}

			for ( int i=0; i<imagesToDestroy.Count; i++) {
				if ( imagesToDestroy[i] != null ) { // 这应该永远不会为false，但让我们保持安全
					CleanDestroy<Image>(imagesToDestroy[i].gameObject);
				}
			}

		}

		/// <summary>HideAll实际上不使用任何参数</summary>
		public void HideAllSprites() {
			HideSprite( "*" );
			actors.Clear();
			sprites.Clear();
		}

		/// <summary>Reset实际上不使用任何参数</summary>
		public void ResetScene() {
			bgImage.sprite = null;
			HideAllSprites();
			SetFadeIn(0);
		}

		// 移动精灵用法：<<Move actorOrspriteName, screenPosX=0.5,
		// screenPosY=0.5, moveTime=1.0>> screenPosX和screenPosY是
		// 归一化的屏幕坐标（0.0 - 1.0）moveTime是到达
		// 该位置所需的时间（秒）
		public void MoveSprite(string actorOrSpriteName, string screenPosX="0.5", string screenPosY="0.5", float moveTime = 1) {
			
			var image = FindActorOrSprite( actorOrSpriteName );

			// 获取新的屏幕位置
			Vector2 newPos = new Vector2(0.5f, 0.5f);
			if ( screenPosX != string.Empty && screenPosY != string.Empty) {
				newPos = new Vector2( ConvertCoordinates(screenPosX), ConvertCoordinates(screenPosY) );
			} else if ( screenPosX != string.Empty ) {
				newPos.x = ConvertCoordinates(screenPosX);
			}

			// 现在实际进行移动
			StartCoroutine( MoveCoroutine( image.GetComponent<RectTransform>(), Vector2.Scale(newPos, screenSize), moveTime) );
		}

		/// <summary>翻转精灵，或强制精灵面向某个
		///方向< Move(actorOrSpriteName, xDirection=toggle)</sprite>
		public void FlipSprite(string actorOrSpriteName, string xDirection = "") {
			
			var image = FindActorOrSprite( actorOrSpriteName );


            float direction;

            if (xDirection != string.Empty) {
                direction = Mathf.Sign(ConvertCoordinates(xDirection) - 0.5f);
            }
            else {
                direction = Mathf.Sign(image.rectTransform.localScale.x) * -1f;
            }

			image.rectTransform.localScale = new Vector3( 
                direction * Mathf.Abs(image.rectTransform.localScale.x), 
                image.rectTransform.localScale.y, 
                image.rectTransform.localScale.z 
            );
		}

		/// <summary>Shake(actorName或spriteName, strength=0.5)</summary>
		public void ShakeSprite(string actorOrSpriteName, float shakeStrength = 0.5f) {
			
			var findShakeTarget = FindActorOrSprite( actorOrSpriteName );
			if ( findShakeTarget != null ) {
				StartCoroutine( SetShake( findShakeTarget.rectTransform, shakeStrength ) );
			}
		}

		/// <summary>ShakeCamera(strength=0.5) 控制整个摄像头的震动</summary>
		public void ShakeCamera(float shakeStrength = 0.5f) {
			if (dialogPanel != null) {
				StartCoroutine( SetCameraShake( shakeStrength ) );
			} else {
				Debug.LogError("DialogPanel is not assigned in VNManager!");
			}
		}

		/// <summary>PlayAudio( soundName,volume,"loop" )...
		/// PlayAudio(soundName,1.0)以100%音量播放soundName一次...
		/// 如果第三个参数是单词"loop"，它将循环播放
		/// "volume"是0.0到1.0之间的数字
		/// "loop"是单词"loop"（或"true"），
		/// 告诉声音循环播放</summary>
		public void PlayAudio(string soundName, float volume = 1, string loop = "") {
			
			var audioClip = FetchAsset<AudioClip>(soundName);
			// 检测音量设置
			
            if ( volume <= 0.01f ) {
                Debug.LogWarningFormat(this, "VN管理器正在以非常低的音量({1})播放声音{0}，请注意", soundName, volume );
            }
			
			// 检测循环设置
			bool shouldLoop = loop.Contains("loop") || loop.Contains("true");			
			
			// 实例化AudioSource并配置它（不要使用
			// AudioSource.PlayOneShot，因为我们还想要使用
			// <<StopAudio>>并中断它的选项）
			var newAudioSource = Instantiate<AudioSource>( genericAudioSource, genericAudioSource.transform.parent );
			newAudioSource.name = audioClip.name;
			newAudioSource.clip = audioClip;
			newAudioSource.volume *= volume;
			newAudioSource.loop = shouldLoop;
			newAudioSource.Play();
			sounds.Add(newAudioSource);

			// 如果它不循环，让我们为这个声音设置一个最大生命周期
			if ( shouldLoop == false ) {
				StartCoroutine( SetDestroyTime( newAudioSource, audioClip.length ) );
			}
		}

		/// <summary>根据声音名称停止声音播放，无论它是否
		/// 循环</summary>
		public void StopAudio(string soundName) {
			
			// 让我们现在用草率的方式做这个，并假设
			// 只有一个这样的对象
			AudioSource toDestroy = null;
			foreach ( var audioObject in sounds ) {
				if (audioObject.name == soundName) {
					toDestroy = audioObject;
					break;
				}
			}

			// 再次检查是否有任何audioSource要销毁，因为
			// 它可能已经被销毁了
			if ( toDestroy != null ) {
				CleanDestroy<AudioSource>( toDestroy.gameObject );
			} else {
				Debug.LogWarningFormat(this, "VN管理器尝试<<StopAudio {0}>>但找不到任何正在播放的声音\"{0}\"。请检查名称，或者它可能已经停止了。", soundName );
			}
		}

		/// <summary>停止所有当前正在播放的声音，实际上
		/// 不接受任何参数</summary>
		public void StopAudioAll() {
			var toStop = new List<AudioSource>();
			foreach (var audioSrc in sounds ) {
				toStop.Add( audioSrc );
			}
			foreach ( var stopThis in toStop ) {
				StopAudio( stopThis.name );
			}
		}

		/// <summary>典型的屏幕淡入淡出效果，适合过渡？
		/// 用法：Fade( #hexcolor, startAlpha, endAlpha, fadeTime
		/// )</summary>
		public void SetFade(string fadeColorHex, float startAlpha = 0, float endAlpha = 1, float fadeTime = 1) {
			// 获取颜色
			
            if (ColorUtility.TryParseHtmlString( fadeColorHex, out var fadeColor ) == false ) {
				Debug.LogErrorFormat( this, "VN管理器<<Fade>>无法将[{0}]解析为HTML十六进制颜色...它应该看起来像[#FFFFFF]或[##FFCC00FF]，或者一些关键字也可以工作，如[black]或[red]", fadeColorHex );
				fadeColor = Color.magenta;
			}

			// 执行淡入淡出
			StartCoroutine( FadeCoroutine( fadeColor, startAlpha, endAlpha, fadeTime ) );
		}

		/// <summary>方便进行简单的淡入，无论之前的
		/// 淡入淡出颜色或alpha是什么</summary>
		public void SetFadeIn(float fadeTime = 1) {
			
			// 执行淡入
			StartCoroutine( FadeCoroutine( fadeBG.color, -1f, 0f, fadeTime ) );
		}

		/// <summary>平移相机。用法：CameraOffset(xPos, yPos,
		/// moveTime)</summary>
		/// 0, 0是默认中心
		public void SetCameraOffset(string xPos = "", string yPos = "", float moveTime = 0.25f) {
			
			Vector2 newOffset = Vector2.zero;
			if ( xPos != string.Empty && yPos != string.Empty ) {
				newOffset = new Vector2( ConvertCoordinates(xPos) - 0.5f, ConvertCoordinates(xPos) - 0.5f);
			} else if ( xPos != string.Empty ) {
				newOffset.x = ConvertCoordinates(xPos) - 0.5f;
			}

			// 因为我们使用UI叠加层，没有实际的"相机"
			// 所以我们通过移动"Sprites"游戏对象容器来
			// 实现假相机滚动
			var parent = genericSprite.transform.parent.GetComponent<RectTransform>();
			var newPos = Vector2.Scale( new Vector2(0.5f, 0.5f) - newOffset, screenSize );
			StartCoroutine( MoveCoroutine( parent, newPos, moveTime ) );
		}

        #endregion



        #region Utility

        public override void RunLine(LocalizedLine dialogueLine, System.Action onDialogueLineFinished)
        {
            var actorName = dialogueLine.CharacterName;

            if (string.IsNullOrEmpty(actorName) == false && actors.ContainsKey(actorName)) {
                HighlightSprite(actors[actorName].actorImage);
				nameplateBG.color = actors[actorName].actorColor;
                nameplateBG.gameObject.SetActive(true);
            } else {
                nameplateBG.gameObject.SetActive(false);
            }

            onDialogueLineFinished();
        }

		public void HighlightSprite (Image sprite) {
			if (sprite == null) return; // 如果精灵为空，直接返回
			StopCoroutine( "HighlightSpriteCoroutine" ); // 使用StartCoroutine(string)重载，这样我们就可以停止和启动协程（否则它不起作用？）
			StartCoroutine( HighlightSpriteCoroutine(sprite) );
		}

		// 由HighlightSprite调用
		IEnumerator HighlightSpriteCoroutine (Image highlightedSprite) {
			if (highlightedSprite == null) yield break; // 如果精灵为空，直接退出协程
			float t = 0f;
			// 随着时间的推移，逐渐将精灵更改为"正常"或
			// "高亮"
			while ( t < 1f ) {
				t += Time.deltaTime / 2f;
				foreach ( var spr in sprites ) {
					if (spr == null) continue; // 跳过空精灵
					Vector3 regularScalePreserveXFlip = new Vector3( Mathf.Sign(spr.transform.localScale.x), 1f, 1f);
					if ( spr != highlightedSprite) { // 设置回正常
						spr.transform.localScale = Vector3.MoveTowards( spr.transform.localScale, regularScalePreserveXFlip, Time.deltaTime );
						spr.color = Color.Lerp( spr.color, defaultTint, Time.deltaTime * 5f );
					} else { // 稍微大一点/亮一点
						spr.transform.localScale = Vector3.MoveTowards( spr.transform.localScale, regularScalePreserveXFlip * 1.05f, Time.deltaTime );
						spr.color = Color.Lerp( spr.color, highlightTint, Time.deltaTime * 5f );
						spr.transform.SetAsLastSibling();
					}
				}
				yield return 0;
			}
		}

		IEnumerator MoveCoroutine(RectTransform transform, Vector2 newAnchorPos, float moveTime ) {
			Vector2 startPos = transform.anchoredPosition;
			float t = 0f;
			while (t < 1f ) {
				t += Time.deltaTime / Mathf.Max(0.001f, moveTime); // Math.Max防止除以零错误
				transform.anchoredPosition = Vector2.Lerp( startPos, newAnchorPos, t);
				yield return 0;
			}
		}

		IEnumerator FadeCoroutine(Color fadeColor, float startAlpha, float endAlpha, float fadeTime) {
			Color startColor = fadeColor;
			if ( startAlpha >= 0f ) { // 如果startAlpha是-1f，这意味着只使用已经存在的任何内容
				startColor.a = startAlpha;
			} else {
				startColor = fadeBG.color;
			}
			fadeColor.a = endAlpha;
			float t = 0f;
			while ( t < 1f ) {
				t += Time.deltaTime / Mathf.Max(0.001f, fadeTime); // Math.Max防止除以零错误
				fadeBG.color = Color.Lerp( startColor, fadeColor, t );
				yield return 0;
			}
		}

		Image SetSpriteActual(string spriteName, Vector2 position) {
			var newSpriteObject = Instantiate<Image>(genericSprite, genericSprite.transform.parent);
			sprites.Add(newSpriteObject);
			newSpriteObject.name = spriteName;
			newSpriteObject.sprite = FetchAsset<Sprite>( spriteName );
			newSpriteObject.SetNativeSize();
			newSpriteObject.rectTransform.anchoredPosition = Vector2.Scale( position, screenSize );
			return newSpriteObject;
		}

		// TODO: 更改为Image[]并获取所有有效结果？
		Image FindActorOrSprite(string actorOrSpriteName) {
			if ( actors.ContainsKey( actorOrSpriteName ) ) {
				return actors[actorOrSpriteName].actorImage;
			} else { // 或者它是一个通用精灵？
				foreach ( var sprite in sprites ) { // 懒惰的精灵名称搜索
					if ( sprite.name == actorOrSpriteName ) {
						return sprite;
					}
				}
				Debug.LogErrorFormat(this, "VN管理器找不到名称为\"{0}\"的演员或精灵，可能是拼写错误或精灵已被隐藏/销毁", actorOrSpriteName );
				return null;
			}
		}

		// 震动RectTransform（通常是精灵）
		IEnumerator SetShake( RectTransform thingToShake, float shakeStrength = 0.5f ) {
			var startPos = thingToShake.anchoredPosition;
			while ( shakeStrength > 0f ) {
				shakeStrength -= Time.deltaTime;
				float shakeDistance = Mathf.Clamp( shakeStrength * 69f, 0f, 69f);
				float shakeFrequency = Mathf.Clamp( shakeStrength * 5f, 0f, 5f);
				thingToShake.anchoredPosition = startPos + shakeDistance * new Vector2( Mathf.Sin(Time.time * shakeFrequency), Mathf.Sin(Time.time * shakeFrequency + 17f) * 0.62f );
				yield return 0;
			}
			thingToShake.anchoredPosition = startPos;
		}

		// 定时销毁...不能使用Destroy( gameObject, timeDelay )
		// 因为它可能通过<<StopAudio>>或
		// 其他方式提前被销毁，我们也想从列表中删除引用
		IEnumerator SetDestroyTime(AudioSource destroyThis, float timeDelay) {
			float timer = timeDelay;
			while ( timer > 0f ) {
				if ( destroyThis == null ) { break; } // 它可能已经被销毁了，所以让我们确保一下
				if ( destroyThis.isPlaying ) {
					timer -= Time.deltaTime;
				}
				yield return 0;
			}
			if ( destroyThis != null ) { // 它可能已经被销毁了，所以让我们确保一下
				CleanDestroy<AudioSource>( destroyThis.gameObject );
			}
		}

		// CleanDestroy还会从
		// sprites或sounds中删除对gameObject的任何引用
		void CleanDestroy<T>( GameObject destroyThis ) {
			if ( typeof(T) == typeof(AudioSource) ) {
				sounds.Remove( destroyThis.GetComponent<AudioSource>() );
			} else if ( typeof(T) == typeof(Image) ) {
				sprites.Remove( destroyThis.GetComponent<Image>() );
			}

			Destroy( destroyThis );
		}

		// 将"left"或"right"等单词转换为
		// 等效位置数字的实用函数
		float ConvertCoordinates(string coordinate) {
			// 首先，是否有人以这个坐标命名？我们将使用
			// X位置
			if ( actors.ContainsKey(coordinate) ) {
				return actors[coordinate].rectTransform.anchoredPosition.x / screenSize.x;
			}

			// 接下来，让我们看看他们是否使用了位置关键字
			var labelCoordinate = coordinate.ToLower().Replace(" ", "").Replace("_", "").Replace("-", "");
			switch ( labelCoordinate ) {
				case "leftedge":
				case "bottomedge":
				case "loweredge":
					return 0f;
				case "left":
				case "bottom":
				case "lower":
					return 0.25f;
				case "center":
				case "middle":
					return 0.5f;
				case "right":
				case "top":
				case "upper":
					return 0.75f;
				case "rightedge":
				case "topedge":
				case "upperedge":
					return 1f;
				case "offleft":
				    return -0.33f;
				case "offright":
				    return 1.33f;
			}

			// 如果这些都不起作用，那么让我们尝试将其解析为
			// 数字
            float x;
            if (float.TryParse(coordinate, out x))
            {
                return x;
            }
            else
            {
                Debug.LogErrorFormat(this, "VN管理器无法转换位置[{0}]...它必须是对齐方式（left, center, right, 或 top, middle, bottom）或值（如0.42表示42%）", coordinate);
                return -1f;
            }

        }

		// 在\Resources\中查找资源的实用函数
		// 或通过数组手动加载
		T FetchAsset<T>( string assetName ) where T : UnityEngine.Object {
			// 首先，检查它是否是手动加载的资源，使用
			// 手动数组检查...这很混乱，但我想不出更好的
			// 方法
			if ( typeof(T) == typeof(Sprite) ) {
				foreach ( var spr in loadSprites ) {
					if (spr.name == assetName) {
						return spr as T;
					}
				}
			} else if ( typeof(T) == typeof(AudioClip) ) {
				foreach ( var ac in loadAudio ) {
					if ( ac.name == assetName ) {
						return ac as T;
					}
				}
			}

			// 默认情况下，我们已经将所有Resources资源加载到资源
			// 数组中，但如果你不想那样，那么取消注释
			// 这个，等等。if ( useResourcesFolders ) {var newAsset =
			// Resources.Load<T>(assetName); if ( newAsset != null )
			// {return newAsset;
			//  }
			// }

			Debug.LogErrorFormat(this, "VN管理器找不到资源[{0}]...可能是拼写错误，或者没有作为{1}导入？", assetName, typeof(T).ToString() );
			return null; // 没有找到任何匹配的资源
		}

		// 对话框震动协程
		IEnumerator SetCameraShake(float shakeStrength = 0.5f) {
			var startPos = dialogPanel.anchoredPosition;
			while ( shakeStrength > 0f ) {
				shakeStrength -= Time.deltaTime;
				float shakeDistance = Mathf.Clamp( shakeStrength * 69f, 0f, 69f);
				float shakeFrequency = Mathf.Clamp( shakeStrength * 5f, 0f, 5f);
				dialogPanel.anchoredPosition = startPos + shakeDistance * new Vector2( 
					Mathf.Sin(Time.time * shakeFrequency), 
					Mathf.Sin(Time.time * shakeFrequency + 17f) * 0.62f 
				);
				yield return 0;
			}
			dialogPanel.anchoredPosition = startPos;
		}

		#endregion
    } // 结束类

	/// <summary>
	/// 存储演员的数据（精灵引用和颜色），可以
	/// 根据需要扩展
	/// </summary>
	[System.Serializable]
	public class VNActor {
		public Image actorImage;
		public Color actorColor;
		public RectTransform rectTransform { get { return actorImage?.rectTransform; } }
		public GameObject gameObject { get { return actorImage?.gameObject; } }

		public VNActor( Image actorImage, Color actorColor ) {
			this.actorImage = actorImage;
			this.actorColor = actorColor;
		}
	}

	// 来自
	// https://www.codeproject.com/Articles/11556/Converting-Wildcards-to-Regexes
	// 作者：Rei Miyasaka
    class Wildcard : Regex {
        public Wildcard(string pattern) : base(WildcardToRegex(pattern)) { }

        public Wildcard(string pattern, RegexOptions options) : base(WildcardToRegex(pattern), options) { }

        public static string WildcardToRegex(string pattern) {
            return "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
        }
    }

} // 结束命名空间
