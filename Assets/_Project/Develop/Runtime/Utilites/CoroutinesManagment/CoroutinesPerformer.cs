using System.Collections;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilites.CoroutinesManagment
{
    public class CoroutinesPerformer : MonoBehaviour, ICoroutinesPerformer
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public Coroutine StartPerform(IEnumerator coroutine)
            => StartCoroutine(coroutine);

        public void StopPerform(Coroutine coroutine)
            => StopCoroutine(coroutine);
    }
}