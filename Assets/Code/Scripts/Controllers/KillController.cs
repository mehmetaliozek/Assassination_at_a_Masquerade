using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillController : MonoBehaviour
{
    [SerializeField]
    private Transform killPoint;

    [SerializeField]
    private LayerMask visitorLayer;

    [SerializeField]
    private GameObject end;

    public void Kill()
    {
        var hit = Physics.OverlapSphere(killPoint.position, 1f, visitorLayer);
        if (hit.Length != 0)
        {
            if (hit[0].gameObject.TryGetComponent(out Visitor visitor))
            {
                var x = visitor.GetComponentsInChildren<Transform>().Where(w => w.name == "arrow").FirstOrDefault();
                if (x != null)
                {
                    end.SetActive(true);
                    StartCoroutine(Delay());
                }
            }
        }
    }

    IEnumerator Delay() {
        yield return new WaitForSeconds(2.5f);
        SceneManager.LoadScene(0);
    }
}