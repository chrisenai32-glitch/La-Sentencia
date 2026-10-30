using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeIn : MonoBehaviour
{
    [Header("Configuração do Fade")]
    [SerializeField] private float duracaoFade = 1.5f;
    [SerializeField] private float atrasoAntesDoFade = 0.3f; // espera um pouco antes de começar
    [SerializeField] private bool desativarAoTerminar = true; // desativa o objeto após o fade (economiza performance)

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f; // começa totalmente preto/opaco
        canvasGroup.blocksRaycasts = true; // bloqueia cliques durante o fade
    }

    private void Start()
    {
        StartCoroutine(FazerFade());
    }

    private IEnumerator FazerFade()
    {
        yield return new WaitForSeconds(atrasoAntesDoFade);

        float tempoDecorrido = 0f;

        while (tempoDecorrido < duracaoFade)
        {
            tempoDecorrido += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, tempoDecorrido / duracaoFade);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false; // libera cliques no menu

        if (desativarAoTerminar)
            gameObject.SetActive(false);
    }
}