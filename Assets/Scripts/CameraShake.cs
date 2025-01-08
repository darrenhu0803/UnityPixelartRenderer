using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[System.Serializable]
public class Shake {
    public float DurationInit;
    public float Duration;
    public float Strength;
    public Vector2 Direction;
    public Shake(float duration, float strength, Vector2 direction) {
        this.DurationInit = duration;
        this.Duration = duration;
        this.Strength = strength;
        this.Direction = direction.normalized;
    }
}
public class CameraShake: MonoBehaviour {
    List<Shake> _shakes = new List<Shake>();
    public float MultStrength;//.05f
    public float SinePeriod;//60f
    public Transform offsetQuad;

    private Material _material;

    public void Start() {
        _material = offsetQuad.GetComponent<MeshRenderer>().material;
        _material.SetFloat("_Alpha", 0.1f);
        _material.SetFloat("_Power", 0.1f);
        _material.SetVector("_Offset", new Vector2(0, 0));
    }

    public void ShakeAdd(float strength, Vector2 direction) {
        _shakes.Add(new Shake(Mathf.Log10(strength) * 0.05f, Mathf.Log(strength, 3), direction));
    }
    void Update() {
        //_material.SetVector("Offset", new Vector2(0.5f, 0f));
        //_material.SetFloat("Alpha", 0.5f);
        if (Input.GetMouseButtonDown(0)) {
            ShakeAdd(100f, new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0));
            

        }
        int count = _shakes.Count;
        if (count > 0) {
            Debug.Log("Shake");
            Vector2 offset = Vector2.zero;
            for (int i = count - 1; i >= 0; i--) {
                _shakes[i].Duration -= Time.deltaTime;
                if (_shakes[i].Duration > 0) {
                    offset += (Mathf.Sin(SinePeriod * _shakes[i].Duration) * MultStrength * (_shakes[i].Duration / _shakes[i].DurationInit) * _shakes[i].Strength * _shakes[i].Direction);
                } else {
                    _shakes.RemoveAt(i);
                    if (_shakes.Count == 0) {
                        offset = Vector2.zero;
                        _material.SetVector("_Offset", new Vector2(0, 0));
                    }
                }
            }
            if (offset != Vector2.zero) {
                Vector4 _off = _material.GetVector("_Offset");
                _material.SetVector("_Offset", offset);
                Debug.Log(offset);
            }
        }
    }
}