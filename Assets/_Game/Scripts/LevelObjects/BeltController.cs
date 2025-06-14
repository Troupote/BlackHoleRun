using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace BHR
{
    public class BeltController : MonoBehaviour
    {
        [SerializeField] private Vector2 _speedFork;
        [SerializeField] private Material _rotateMat;
        [SerializeField] private int _randomMatGenerationCount;
        private List<Material> _runtimeMats = new List<Material>();
        private bool _init = false;
        private float _angle = 0f;

        private void Awake()
        {
            InitMats();
            InitRocks();
        }
        private void InitMats()
        {
            _runtimeMats.Clear();
            // Set random mat
            for(int i=0; i<_randomMatGenerationCount;++i)
            {
                Material mat = new Material(_rotateMat);
                _runtimeMats.Add(mat);
                
                mat.SetFloat("_GameTimeScale", 1f);
                mat.SetFloat("_Speed", Random.Range(_speedFork.x, _speedFork.y));
                mat.SetVector("_Direction", new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
            }
        }

        private void InitRocks()
        {
            // Set random mat on each rock
            foreach (Transform rock in transform)
                if (rock.name.Contains("rock", System.StringComparison.OrdinalIgnoreCase))
                    rock.GetComponent<Renderer>().material = _runtimeMats[Random.Range(0, _runtimeMats.Count)];

            _init = true;
        }

        private void SetMats(float angle)
        {
            foreach (Material mat in _runtimeMats)
                mat.SetFloat("_Angle", angle);
        }

        private void Update()
        {
            if (!_init) return;

            _angle += Time.deltaTime * GameManager.Instance.GameTimeScale;

            SetMats(_angle);
        }
    }
}
