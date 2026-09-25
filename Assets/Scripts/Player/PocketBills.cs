using System.Collections;
using UnityEngine;

/// <summary>
/// Muestra los billetes como modelos 3D en el bolsillo del jugador.
/// Cada vez que el MoneyManager pierde un billete, uno de los modelos
/// sale del bolsillo con una animación y desaparece.
/// </summary>
public class PocketBills : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private MoneyManager money;

    [Tooltip("Los modelos de los billetes, en orden. El ÚLTIMO de la lista es el primero que se pierde.")]
    [SerializeField] private Transform[] billModels;

    [Header("Animación al perder un billete")]
    [SerializeField] private bool animateLoss = true;
    [SerializeField] private float animationDuration = 0.6f;
    [Tooltip("Hacia dónde sale el billete, en espacio local del bolsillo.")]
    [SerializeField] private Vector3 pullOutOffset = new Vector3(0f, 0.25f, 0.1f);
    [SerializeField] private float spinDegrees = 360f;

    [Header("Feedback opcional")]
    [SerializeField] private AudioSource lossAudio;
    [SerializeField] private ParticleSystem lossParticles;

    private void Start()
    {
        if (money == null)
            money = MoneyManager.Instance;

        if (money == null)
        {
            Debug.LogWarning("PocketBills: no hay MoneyManager en la escena.");
            return;
        }

        money.BillLost += OnBillLost;
        SyncWithMoney();
    }

    private void OnDestroy()
    {
        if (money != null)
            money.BillLost -= OnBillLost;
    }

    /// <summary>Muestra exactamente tantos billetes como tenga el jugador.</summary>
    public void SyncWithMoney()
    {
        if (money == null || billModels == null)
            return;

        for (int i = 0; i < billModels.Length; i++)
        {
            if (billModels[i] != null)
                billModels[i].gameObject.SetActive(i < money.Bills);
        }
    }

    private void OnBillLost(int remaining, string reason)
    {
        // El billete que se acaba de perder es el del índice "remaining"
        if (billModels == null || remaining < 0 || remaining >= billModels.Length)
            return;

        Transform bill = billModels[remaining];
        if (bill == null)
            return;

        if (lossAudio != null)
            lossAudio.Play();

        if (lossParticles != null)
        {
            lossParticles.transform.position = bill.position;
            lossParticles.Play();
        }

        if (animateLoss)
            StartCoroutine(PullOut(bill));
        else
            bill.gameObject.SetActive(false);
    }

    private IEnumerator PullOut(Transform bill)
    {
        Vector3 startPos = bill.localPosition;
        Quaternion startRot = bill.localRotation;
        Vector3 startScale = bill.localScale;

        float t = 0f;
        while (t < animationDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / animationDuration);

            bill.localPosition = startPos + pullOutOffset * k;
            bill.localRotation = startRot * Quaternion.Euler(0f, spinDegrees * k, 0f);
            bill.localScale = startScale * (1f - k);

            yield return null;
        }

        bill.gameObject.SetActive(false);

        // Dejarlo como estaba por si se reinicia sin recargar la escena
        bill.localPosition = startPos;
        bill.localRotation = startRot;
        bill.localScale = startScale;
    }
}