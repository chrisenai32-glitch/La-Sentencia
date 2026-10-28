using UnityEngine;
using UnityEngine.UI;

public class CursorCustomizado : MonoBehaviour
{
    [SerializeField] private Image imagem;
    [SerializeField] private Sprite spriteNormal;
    [SerializeField] private Sprite spriteClicando;

    [SerializeField] private Vector2 offset = new Vector2(10, -10);
    [SerializeField] private Vector2 tamanho = new Vector2(100, 100);
    [SerializeField] private float velocidadeSegue = 20f;

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 posicaoAlvo;
    private bool emModoTeclado = false;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // Busca o Canvas pai
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();

        Debug.Log("Canvas encontrado: " + (canvas != null));
        Debug.Log("RectTransform: " + (rectTransform != null));
        Debug.Log("Image: " + (imagem != null));

        if (rectTransform != null)
            rectTransform.sizeDelta = tamanho;

        if (imagem != null)
        {
            imagem.color = Color.white;
            if (spriteNormal != null)
                imagem.sprite = spriteNormal;
        }

        Cursor.visible = false;
    }

    private void Update()
    {
        if (rectTransform == null || canvas == null)
            return;

        if (!emModoTeclado)
        {
            RectTransform canvasRect = (RectTransform)canvas.transform;

            // Em Screen Space Overlay, a câmera deve ser null.
            // Em outros modos (Camera/World Space), usa a worldCamera do canvas.
            Camera camera = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

            Vector2 localPoint;
            bool sucesso = UnityEngine.RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                camera,
                out localPoint
            );

            if (sucesso)
                posicaoAlvo = localPoint + offset;
        }

        // Move a seta suavemente
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            posicaoAlvo,
            Time.deltaTime * velocidadeSegue
        );

        // Muda sprite ao clicar
        if (imagem != null)
        {
            if (Input.GetMouseButtonDown(0) && spriteClicando != null)
                imagem.sprite = spriteClicando;
            if (Input.GetMouseButtonUp(0) && spriteNormal != null)
                imagem.sprite = spriteNormal;
        }
    }

    public void PosicionarNosBotao(Button botao, float distanciaX = -60f)
    {
        if (botao == null || rectTransform == null) return;

        emModoTeclado = true;
        if (imagem != null && spriteNormal != null)
            imagem.sprite = spriteNormal;

        RectTransform botaoRect = botao.GetComponent<RectTransform>();
        if (botaoRect != null)
            posicaoAlvo = botaoRect.anchoredPosition + new Vector2(distanciaX, 0);
    }

    public void VoltarAoMouse()
    {
        emModoTeclado = false;
    }
}