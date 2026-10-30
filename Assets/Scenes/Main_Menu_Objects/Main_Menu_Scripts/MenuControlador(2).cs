using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MenuControlador : MonoBehaviour
{
    [Header("Botões (ordem: Novo Jogo, Continuar, Configurações, Sair)")]
    [SerializeField] private Button[] listaBotoes;
    [SerializeField] private TMP_Text[] listaTextos;

    [Header("Painéis")]
    [SerializeField] private GameObject painelMenuPrincipal;
    [SerializeField] private GameObject painelAjustes;

    [Header("Cursor Customizado")]
    [SerializeField] private CursorCustomizado setaCursor;
    [SerializeField] private float distanciaSetaBotao = -60f;

    [Header("Cenas")]
    [SerializeField] private string nomeCenaJogo = "Jogo";
    [SerializeField] private FadeOutCena fadeOutCena;

    [Header("Cores")]
    [SerializeField] private Color corTextoNormal = Color.white;
    [SerializeField] private Color corTextoSelecionado = Color.yellow;
    [SerializeField] private Color corTextoBloqueado = new Color(1f, 1f, 1f, 0.3f);

    [Header("Áudio")]
    [SerializeField] private AudioSource fonteAudio;
    [SerializeField] private AudioClip clipeSelecionar;
    [Tooltip("Um som de confirmação para cada botão, na MESMA ORDEM de 'Lista Botões' (0=Novo Jogo, 1=Continuar, 2=Configurações, 3=Sair)")]
    [SerializeField] private AudioClip[] clipesConfirmarPorBotao;
    [SerializeField] private float intervaloMinimoEntreSons = 0.08f; // evita som "metralhadora"

    private float momentoUltimoSom = -999f;

    private const int OPCAO_NOVO_JOGO = 0, OPCAO_CONTINUAR = 1, OPCAO_AJUSTES = 2, OPCAO_SAIR = 3;

    private int indiceAtual;
    private bool existeSave;
    private bool navegandoPorTeclado = false;

    private string CaminhoArquivoSave => Path.Combine(Application.persistentDataPath, "save.json");

    private void Start()
    {
        existeSave = File.Exists(CaminhoArquivoSave);

        listaBotoes[OPCAO_CONTINUAR].interactable = existeSave;

        for (int i = 0; i < listaBotoes.Length; i++)
        {
            int idx = i;
            listaBotoes[i].onClick.AddListener(() => { indiceAtual = idx; ConfirmarOpcao(); });

            var gatilho = listaBotoes[i].gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            var entrada = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
            };
            entrada.callback.AddListener(_ =>
            {
                if (listaBotoes[idx].interactable)
                {
                    indiceAtual = idx;
                    navegandoPorTeclado = false;
                    setaCursor.VoltarAoMouse();
                    AtualizarVisual();
                    TocarClipeSelecionar();
                }
            });
            gatilho.triggers.Add(entrada);
        }

        painelAjustes.SetActive(false);
        painelMenuPrincipal.SetActive(true);

        indiceAtual = OPCAO_NOVO_JOGO;
        AtualizarVisual();
    }

    private void Update()
    {
        if (painelAjustes.activeSelf)
        {
            if (TeclaVoltar()) FecharAjustes();
            return;
        }

        // navegandoPorTeclado é ativado ANTES de mover, assim a seta já se
        // posiciona corretamente desde a primeira tecla pressionada
        if (TeclaCima())  { navegandoPorTeclado = true; MoverSelecao(-1); TocarClipeSelecionar(); }
        if (TeclaBaixo()) { navegandoPorTeclado = true; MoverSelecao(1);  TocarClipeSelecionar(); }
        if (TeclaConfirmar()) ConfirmarOpcao();
    }

    private void MoverSelecao(int direcao)
    {
        int tentativas = listaBotoes.Length;
        do
        {
            indiceAtual = (indiceAtual + direcao + listaBotoes.Length) % listaBotoes.Length;
            tentativas--;
        }
        while (!listaBotoes[indiceAtual].interactable && tentativas > 0);

        AtualizarVisual();
    }

    private void AtualizarVisual()
    {
        for (int i = 0; i < listaTextos.Length; i++)
        {
            if (!listaBotoes[i].interactable) listaTextos[i].color = corTextoBloqueado;
            else listaTextos[i].color = (i == indiceAtual) ? corTextoSelecionado : corTextoNormal;
        }

        if (navegandoPorTeclado)
            setaCursor.PosicionarNosBotao(listaBotoes[indiceAtual], distanciaSetaBotao);
    }

    private void ConfirmarOpcao()
    {
        if (!listaBotoes[indiceAtual].interactable) return;

        TocarClipeConfirmar(indiceAtual);

        switch (indiceAtual)
        {
            case OPCAO_NOVO_JOGO:
                if (File.Exists(CaminhoArquivoSave)) File.Delete(CaminhoArquivoSave);
                fadeOutCena.IniciarFadeETrocarCena(nomeCenaJogo);
                break;

            case OPCAO_CONTINUAR:
                fadeOutCena.IniciarFadeETrocarCena(nomeCenaJogo);
                break;

            case OPCAO_AJUSTES:
                painelMenuPrincipal.SetActive(false);
                painelAjustes.SetActive(true);
                break;

            case OPCAO_SAIR:
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }

    public void FecharAjustes()
    {
        painelAjustes.SetActive(false);
        painelMenuPrincipal.SetActive(true);
        navegandoPorTeclado = false;
        setaCursor.VoltarAoMouse();
        AtualizarVisual();
    }

    private void TocarClipeSelecionar()
    {
        if (fonteAudio == null || clipeSelecionar == null) return;
        if (Time.unscaledTime - momentoUltimoSom < intervaloMinimoEntreSons) return;

        momentoUltimoSom = Time.unscaledTime;
        fonteAudio.PlayOneShot(clipeSelecionar);
    }

    private void TocarClipeConfirmar(int indiceBotao)
    {
        if (fonteAudio == null) return;
        if (clipesConfirmarPorBotao == null || indiceBotao >= clipesConfirmarPorBotao.Length) return;

        AudioClip clipe = clipesConfirmarPorBotao[indiceBotao];
        if (clipe != null)
            fonteAudio.PlayOneShot(clipe); // confirmar não precisa de cooldown, é um clique só
    }

#if ENABLE_INPUT_SYSTEM
    private bool TeclaCima()      => Keyboard.current != null && (Keyboard.current.upArrowKey.wasPressedThisFrame   || Keyboard.current.wKey.wasPressedThisFrame)
                                  || Gamepad.current  != null && Gamepad.current.dpad.up.wasPressedThisFrame;
    private bool TeclaBaixo()     => Keyboard.current != null && (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
                                  || Gamepad.current  != null && Gamepad.current.dpad.down.wasPressedThisFrame;
    private bool TeclaConfirmar() => Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame     || Keyboard.current.spaceKey.wasPressedThisFrame)
                                  || Gamepad.current  != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
    private bool TeclaVoltar()    => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame
                                  || Gamepad.current  != null && Gamepad.current.buttonEast.wasPressedThisFrame;
#else
    private bool TeclaCima()      => Input.GetKeyDown(KeyCode.UpArrow)   || Input.GetKeyDown(KeyCode.W);
    private bool TeclaBaixo()     => Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
    private bool TeclaConfirmar() => Input.GetKeyDown(KeyCode.Return)    || Input.GetKeyDown(KeyCode.Space);
    private bool TeclaVoltar()    => Input.GetKeyDown(KeyCode.Escape);
#endif
}
