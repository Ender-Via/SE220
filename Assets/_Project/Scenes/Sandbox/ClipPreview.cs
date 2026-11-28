using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Woodcutter.Dev
{
    /// <summary>
    /// Chỉ dùng để xem thử art: phát lặp lại 1 AnimationClip trên SpriteRenderer, không cần Animator Controller.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class ClipPreview : MonoBehaviour
    {
        [SerializeField] private AnimationClip clip;
        [SerializeField] private float speed = 1f;

        private PlayableGraph graph;
        private AnimationClipPlayable playable;

        private void OnEnable()
        {
            if (clip == null)
                return;

            playable = AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), clip, out graph);
            playable.SetSpeed(speed);
        }

        private void Update()
        {
            // Chỉ tua lại clip KHÔNG bật Loop Time (Hit, Dead...) để dễ xem.
            // Clip có Loop Time (Idle, Walk...) đã tự lặp, tua thêm sẽ làm nhảy khung.
            if (clip != null && !clip.isLooping && graph.IsValid() && playable.GetTime() >= clip.length + 0.4f)
                playable.SetTime(0);
        }

        private void OnDisable()
        {
            if (graph.IsValid())
                graph.Destroy();
        }
    }
}
