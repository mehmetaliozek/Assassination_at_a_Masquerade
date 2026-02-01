using UnityEngine;

public class Player : MonoBehaviour
{
    private MovementController movementController;
    private AnimationController animationController;
    private ThreatController threatController;
    private KillController killController;
    private WebSocketService wsService;
    private RadarHandler radarHandler;
    private Vector2 currentMoveInput;

    public string daggerUrl;
    private Dagger dagger = new Dagger();

    public string radarUrl;

    public bool isController = false;

    //public float maxHassasiyet = 500f;
    //public float resetThreshold = 2000f;

    //private float lastX, lastY;
    //private bool firstData = true;
    //private float sensorInputX, sensorInputZ;

    private async void Start()
    {
        movementController = GetComponent<MovementController>();
        animationController = GetComponent<AnimationController>();
        threatController = GetComponent<ThreatController>();
        killController = GetComponent<KillController>();

        if (!isController) return;
        StartCoroutine(NetworkManager.GetJsonData<Dagger>(
            daggerUrl,
            (data) =>
            {
                dagger = data;
            },
            (error) => { Debug.Log(error); })
        );

        // 1. Genel servisi oluştur
        wsService = new WebSocketService();
        // 2. Özel işleyiciyi oluştur
        radarHandler = new RadarHandler();

        // Mesaj geldiğinde ne yapılacağını tanımla
        wsService.OnMessageReceived += (msg) =>
        {
            currentMoveInput = radarHandler.ProcessRawMessage(msg);
        };

        wsService.OnError += (err) => Debug.LogError("Soket Hatası: " + err);

        // 3. Bağlan
        await wsService.Connect(radarUrl);
    }

    private void Update()
    {

        float x = (Input.GetAxis("Horizontal") + currentMoveInput.x) * (isController ? -1 : 1);
        float z = (Input.GetAxis("Vertical") + currentMoveInput.y) * (isController ? -1 : 1);

        x = Mathf.Clamp(x, -1f, 1f);
        z = Mathf.Clamp(z, -1f, 1f);

        movementController.Move(x, z);
        animationController.Walk(movementController.MagnitudeVelocity);

        switch (dagger.Sum)
        {
            case 1:
                if ((Input.GetKeyDown(KeyCode.Mouse0) || dagger.isDagger == 1) && animationController.GetThreat() == false && animationController.GetKill() == false)
                {
                    threatController.Threat();
                    animationController.Threat();
                }
                break;
            case 2:
                if ((Input.GetKeyDown(KeyCode.Mouse1) || dagger.attack == 1) && animationController.GetKill() == false) //&& threatController.i == 3
                {
                    killController.Kill();
                    animationController.ThreatFalse();
                    animationController.Kill();
                }
                break;
        }
        //if ((Input.GetKeyDown(KeyCode.Mouse1) || dagger.attack == 1) && animationController.GetKill() == false) //&& threatController.i == 3
        //{
        //    killController.Kill();
        //    animationController.Kill();
        //}


        //if ((Input.GetKeyDown(KeyCode.Mouse0) || (dagger.isDagger == 1 && dagger.attack == 0)) && animationController.GetThreat() == false)
        //{
        //    threatController.Threat();
        //    animationController.Threat();
        //}

    }

    async void OnApplicationQuit()
    {
        await wsService.Disconnect();
    }
}
