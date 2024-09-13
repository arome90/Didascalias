using UnityEngine;

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "DisruptiveAction", menuName = "ScriptableObject/DisruptiveAction", order = 5)]
    public class DisruptiveAction : ScriptableObject
    {
        [Header("Animation & Audio")]
        [Tooltip("Animation that displays the problematic action.")]
        [SerializeField] private AnimationClip _problematicAnimation;
        public AnimationClip ProblematicAnimation => _problematicAnimation;

        [Tooltip("Audio clip for masculine situations.")]
        [SerializeField] private AudioClip _situationAudioMasculine;
        public AudioClip SituationAudioMasculine => _situationAudioMasculine;

        [Tooltip("Audio clip for feminine situations.")]
        [SerializeField] private AudioClip _situationAudioFeminine;
        public AudioClip SituationAudioFeminine => _situationAudioFeminine;

        [Tooltip("Whether laughter is involved in the situation.")]
        [SerializeField] private bool _laughter;
        public bool Laughter => _laughter;

        [Header("Behavior and Timing")]
        [Tooltip("Time it takes for the character to react.")]
        [SerializeField] private float _reactionTime;
        public float ReactionTime => _reactionTime;

        [Tooltip("GameObject that holds the behavior settings.")]
        [SerializeField] private GameObject _behaviorHolder;
        public GameObject BehaviorHolder => _behaviorHolder;

        [Tooltip("Action associated with this disruptive behavior.")]
        [SerializeField] private Actions _action;
        public Actions Action => _action;
    }
}
