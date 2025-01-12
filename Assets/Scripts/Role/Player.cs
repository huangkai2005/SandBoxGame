using System;
using Animancer;
using UnityEngine;

namespace MoonFramework.Test
{
    public class Player : MonoBehaviour
    {
        public AnimancerComponent Animancer;
        [SerializeField] private ClipTransition _walk, _attack;
        private LinearMixerState _states;

        private void Start()
        {
            _states = new LinearMixerState();
            _states.Add(_walk, 0f);
            _states.Add(_attack, 1f);
            Animancer.Play(_states);
        }

        private void Update()
        {
            var hor = Input.GetAxis("Horizontal");
            var ver = Input.GetAxis("Vertical");
            var movement = new Vector3(hor, 0, ver);

            if (movement != Vector3.zero)
            {
                transform.Translate(10f * Time.deltaTime * movement.normalized, Space.Self);
                _states.Parameter = 0f;
            }

            if (Input.GetMouseButton(0))
            {
                _states.Parameter = 1f;
            }
        }

        public void SkillOver()
        {
            
        }
    }
}