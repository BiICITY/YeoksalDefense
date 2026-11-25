using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshPro textMesh;

    [Header("Motion")]
    [SerializeField] private float lifeTime = 0.75f;
    [SerializeField] private float moveSpeed = 1.4f;
    [SerializeField] private float randomOffsetX = 0.25f;
    [SerializeField] private float randomOffsetZ = 0.15f;

    private float timer = 0f;
    private Color startColor;

    private void Awake()
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }
    }

    public void Initialize(string content, Color color)
    {
        if (textMesh != null)
        {
            textMesh.text = content;
            textMesh.color = color;
            startColor = color;
        }

        Vector3 randomOffset = new Vector3(
            Random.Range(-randomOffsetX, randomOffsetX),
            0f,
            Random.Range(-randomOffsetZ, randomOffsetZ)
        );

        transform.position += randomOffset;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        FaceCamera();
        FadeOut();

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void FaceCamera()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        transform.rotation = mainCamera.transform.rotation;
    }

    private void FadeOut()
    {
        if (textMesh == null)
        {
            return;
        }

        float t = timer / lifeTime;
        Color color = startColor;
        color.a = Mathf.Lerp(1f, 0f, t);

        textMesh.color = color;
    }
}