using UnityEngine;
using UnityEngine.Splines;

public class ControleurVitesseSpline : MonoBehaviour
{
public SplineAnimate animationSpline;
public AnimationCurve profilDeVitesse;
public float vitesseMaximum = 15f;

void Update()
{
    float position = animationSpline.NormalizedTime;
    float multiplicateur = profilDeVitesse.Evaluate(position);
    animationSpline.MaxSpeed = vitesseMaximum * multiplicateur;
}
}