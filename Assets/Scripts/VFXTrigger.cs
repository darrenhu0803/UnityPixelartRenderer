
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXTrigger: MonoBehaviour {

    public VisualEffect slash1;
    public VisualEffect slash2;
    public VisualEffect distortion;
    public VisualEffect pre1;
    public VisualEffect pre2;

    public CameraShake cameraShake;

    private bool shake = false;
    private bool beforeSlash = false;
    private float shakeIntensity = 10f;


    private List<float> positionsX = new List<float>();
    private List<float> positionsY = new List<float>();
    private List<float> angles = new List<float>();

    public PolygonGeneration polygonGeneration;

    void Start() {
        shakeIntensity = 10f;
    }

    void Update() {
        if (Input.GetKeyDown("space") && !slash1.HasAnySystemAwake()) {

            pre1.Play();
            pre2.Play();

            shake = true;
            shakeIntensity = -2f;
            beforeSlash = true;


            Invoke("StartSlash", 3f);
            polygonGeneration.Refresh();

        }
        if (slash1.HasAnySystemAwake()) {
            slash1.playRate = Mathf.Lerp(slash1.playRate, 1f, Time.deltaTime * 20f);
            slash2.playRate = slash1.playRate;
        }

        if (shake) {
            cameraShake.ShakeAdd(shakeIntensity > 0f ? shakeIntensity : 0f, new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
        }
        if (beforeSlash) {
            shakeIntensity += 4f * Time.deltaTime;
        }
    }
    
    void StartSlash() {



        pre1.Reinit();
        pre1.Stop();
        pre2.Reinit();
        pre2.Stop();
        StopShake();
        shakeIntensity = 10f;
        beforeSlash = false;

        uint seed = (uint) Mathf.FloorToInt(Random.Range(0, 100000));

        slash1.playRate = 10f;
        AnimationCurve _positionsX = new AnimationCurve();
        AnimationCurve _positionsY = new AnimationCurve();
        AnimationCurve _angles = new AnimationCurve();
        positionsX = new List<float>();
        positionsY = new List<float>();
        angles = new List<float>();

        for (int i = 0; i < 20; i++) {
            float x = Random.Range(-6f, 6f);
            float y = Random.Range(-6f, 6f);
            float angle = Random.Range(0f, 180f);
            positionsX.Add(x);
            positionsY.Add(y);
            angles.Add(angle);
            _positionsX.AddKey(i, x);
            _positionsY.AddKey(i, y);
            _angles.AddKey(i, angle);

        }

        slash1.SetAnimationCurve("PositionsX", _positionsX);
        slash1.SetAnimationCurve("PositionsY", _positionsY);
        slash1.SetAnimationCurve("Angles", _angles);

        slash2.SetAnimationCurve("PositionsX", _positionsX);
        slash2.SetAnimationCurve("PositionsY", _positionsY);
        slash2.SetAnimationCurve("Angles", _angles);


        slash1.Play();



        slash2.playRate = 10f;
        slash2.Play();

        distortion.Play();

        Invoke("StartShake", 5.8f);
        Invoke("StopVFX", 5.9f);
        Invoke("StopShake", 6f);
    }


    void StopVFX() {
        slash1.Reinit();
        slash1.Stop();

        slash2.Reinit();
        slash2.Stop();
    }

    void StartShake() {
        if (!shake) {
            shake = true;
        }

        for (int i = 0; i < 20; i++) {

            polygonGeneration.Cut(new Vector2(positionsX[i], positionsY[i]), angles[i], new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f)), new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f)));

        }

        polygonGeneration.RegenerateChildren();

    }

    void StopShake() {
        if (shake) {
            shake = false;
        }
    }
}
