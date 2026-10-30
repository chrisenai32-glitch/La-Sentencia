using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class FadeOutCena : MonoBehaviour
{
    [Header("Fade visual")]
    [SerializeField] private float duracaoFade = 1.5f;

    [Header("Música de fundo (opcional)")]
    [SerializeField] private AudioSource musicaFundo;

    private CanvasGroup canvasGroup;
    private bool fadeEmAndamento = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>Inicia o fade para preto, diminui a música e troca de cena ao final.</summary>
    public void IniciarFadeETrocarCena(string nomeCena)
    {
        if (fadeEmAndamento) return; // evita clicar várias vezes e disparar múltiplos fades
        fadeEmAndamento = true;
        StartCoroutine(FadeOutRotina(nomeCena));
    }

    private IEnumerator FadeOutRotina(string nomeCena)
    {
        canvasGroup.blocksRaycasts = true; // bloqueia cliques durante a transição

        float volumeInicialMusica = musicaFundo != null ? musicaFundo.volume : 0f;
        float tempoDecorrido = 0f;

        while (tempoDecorrido < duracaoFade)
        {
            tempoDecorrido += Time.unscaledDeltaTime; // unscaled: funciona mesmo se o jogo pausar
            float progresso = tempoDecorrido / duracaoFade;

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progresso);

            if (musicaFundo != null)
                musicaFundo.volume = Mathf.Lerp(volumeInicialMusica, 0f, progresso);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        if (musicaFundo != null)
            musicaFundo.volume = 0f;

        SceneManager.LoadScene(nomeCena);
    }
}
