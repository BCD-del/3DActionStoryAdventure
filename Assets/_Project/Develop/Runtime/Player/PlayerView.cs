using UnityEngine;

namespace Assets._Project.Develop.Runtime.Player
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private Animator _animator;

        private void Update()
        {
            _animator.SetBool("IsRunning", _player.IsRunning);
        }
    }
}

// Rig -> Humanoid and Create Avatar from this model (для первого раза, для новых анимаций from another avatar)