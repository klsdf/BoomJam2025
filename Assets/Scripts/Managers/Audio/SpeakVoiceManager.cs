using UnityEngine;
using MoreMountains.Feedbacks;

namespace BoomJam2025
{
    public class SpeakVoiceManager : MonoBehaviour
    {
        [Header("MMF引用")]
        [SerializeField] private MMFeedbacks streamerMMF;
        [SerializeField] private MMFeedbacks protagonistMMF;
        [SerializeField] private MMFeedbacks narratorMMF;
        [SerializeField] private MMFeedbacks cherryMMF;

        public void StreamerSpeak()
        {
            if (streamerMMF != null)
            {
                streamerMMF.PlayFeedbacks();
            }
        }

        public void ProtagonistSpeak()
        {
            if (protagonistMMF != null)
            {
                protagonistMMF.PlayFeedbacks();
            }
        }

        public void NarratorSpeak()
        {
            if (narratorMMF != null)
            {
                narratorMMF.PlayFeedbacks();
            }
        }

        public void CherrySpeak()
        {
            if (cherryMMF != null)
            {
                cherryMMF.PlayFeedbacks();
            }
        }
    }
} 