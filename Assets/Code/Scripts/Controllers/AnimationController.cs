using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    public void Walk(float velocity)
    {
        animator.SetFloat("Velocity", Mathf.Abs(velocity));
    }

    public void Threat()
    {
        animator.SetBool("Threat", true);
    }

    public void ThreatFalse()
    {
        animator.SetBool("Threat", false);
    }

    public bool GetThreat() => animator.GetBool("Threat");


    public void Kill()
    {
        animator.SetBool("Kill", true);
    }

    public void KillFalse()
    {
        animator.SetBool("Kill", false);
    }

    public bool GetKill() => animator.GetBool("Kill");

}