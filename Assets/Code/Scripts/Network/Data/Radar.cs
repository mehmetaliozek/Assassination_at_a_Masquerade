using System;
using UnityEngine;

[Serializable]
public class RadarData
{
    public float x;
    public float y;
    public float speed;
    public float dist;
    public float ang;
    public int det;
}

public class RadarHandler
{
    private float _lastX, _lastY;
    private bool _firstData = true;

    public float MaxSensitivity = 50f;
    public float ResetThreshold = 500f;

    public Vector2 ProcessRawMessage(string json)
    {
        RadarData data = JsonUtility.FromJson<RadarData>(json);

        if (data.det == 0)
        {
            _firstData = true;
            return Vector2.zero;
        }

        if (_firstData)
        {
            _lastX = data.x; _lastY = data.y;
            _firstData = false;
            return Vector2.zero;
        }

        // --- SENSÖR DÜNYASI (Ham Değişim) ---
        float dx = data.x - _lastX;
        float dy = data.y - _lastY;

        _lastX = data.x;
        _lastY = data.y;

        // Zıplama Kontrolü (Sensörün kendi eksenlerinde yapılmalı)
        if (Mathf.Abs(dx) > ResetThreshold || Mathf.Abs(dy) > ResetThreshold)
        {
            return Vector2.zero;
        }

        // --- UNITY DÜNYASINA AKTARMA (Eşleme Burada Yapılır) ---
        // Senin istediğin: Sensör Y -> Unity X | Sensör X -> Unity Z

        // NOT: Eğer karakter ters yöne giderse başına '-' koy (örn: -dy)
        float unityHorizontal = dy / MaxSensitivity;
        float unityVertical = dx / MaxSensitivity;

        // Filtreleme ve Sınırlandırma
        float finalX = Mathf.Clamp(unityHorizontal, -1f, 1f);
        float finalZ = Mathf.Clamp(unityVertical, -1f, 1f);

        return new Vector2(finalX, finalZ);
    }
}