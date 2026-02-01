using UnityEngine;

public class ThreatController : MonoBehaviour
{
    [SerializeField]
    private Mask Nobble;

    [SerializeField]
    private Mask Witch;

    [SerializeField]
    private Mask Bureaucrat;

    [SerializeField]
    private Transform threatPoint;

    [SerializeField]
    private GameObject arrow;

    [SerializeField]
    private LayerMask visitorLayer;

    public int i = 0;

    public void Threat()
    {
        var hit = Physics.OverlapSphere(threatPoint.position, 1f, visitorLayer);
        if (hit.Length != 0)
        {
            if (hit[0].gameObject.TryGetComponent(out Visitor visitor))
            {
                switch (visitor.GetVisitorType())
                {
                    case VisitorType.Nobble:
                        Nobble.ActiveText(ref i);
                        break;
                    case VisitorType.Witch:
                        Witch.ActiveText(ref i);
                        break;
                    case VisitorType.Bureaucrat:
                        Bureaucrat.ActiveText(ref i);
                        break;
                    default:
                        break;
                }
            }
        }

        if (i == 3)
        {
            arrow.SetActive(true);
        }
    }
}