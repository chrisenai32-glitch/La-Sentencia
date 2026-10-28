using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MenuPrincipal : MonoBehaviour
{
    [Header("Botões (ordem: Novo Jogo, Continuar, Configurações, Sair)")]
    [SerializeField] private Button[] botoes;
    [SerializeField] private TMP_Text[] textos;

    [Header("Painéis")]
    [SerializeField] private GameObject painelMenu;
    [SerializeField] private GameObject painelConfiguracoes;

    [Header("Cursor Customizado")]
    [SerializeField] private CursorCustomizado cursor;
    [SerializeField] private float distanciaSetaBotao = -60f;

    [Header("Cenas")]
    [SerializeField] private string cenaNovoJogo = "Jogo";

    [Header("Cores")]
    [SerializeField] private Color corNormal = Color.white;
    [SerializeField] private Color corSelecionado = Color.yellow;
    [SerializeField] private Color corBloqueado = new Color(1f, 1f, 1f, 0.3f);

    private const int NOVO_JOGO = 0, CONTINUAR = 1, CONFIG = 2, SAIR = 3;

    private int indice;
    private bool temSave;
    private bool usandoTeclado = false;

    private string CaminhoSave => Path.Combine(Application.persistentDataPath, "save.json");

    private void Start()
    {
        temSave = File.Exists(CaminhoSave);

        botoes[CONTINUAR].interactable = temSave;

        for (int i = 0; i < botoes.Length; i++)
        {
            int idx = i;
            botoes[i].onClick.AddListener(() => { indice = idx; Confirmar(); });

            var trigger = botoes[i].gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
            };
            entry.callback.AddListener(_ =>
            {
                if (botoes[idx].interactable)
                {
                    indice = idx;
                    usandoTeclado = false;
                    cursor.VoltarAoMouse();
                    Atualizar();
                }
            });
            trigger.triggers.Add(entry);
        }

        painelConfiguracoes.SetActive(false);
        painelMenu.SetActive(true);

        indice = NOVO_JOGO;
        Atualizar();
    }

    private void Update()
    {
        if (painelConfiguracoes.activeSelf)
        {
            if (Voltar()) FecharConfiguracoes();
            return;
        }

        // usandoTeclado é ativado ANTES de mover, assim a seta já se
        // posiciona corretamente desde a primeira tecla pressionada
        if (Cima()) { usandoTeclado = true; Mover(-1); }
        if (Baixo()) { usandoTeclado = true; Mover(1); }
        if (Confirma()) Confirmar();
    }

    private void Mover(int direcao)
    {
        int tentativas = botoes.Length;
        do
        {
            indice = (indice + direcao + botoes.Length) % botoes.Length;
            tentativas--;
        }
        while (!botoes[indice].interactable && tentativas > 0);

        Atualizar();
    }

    private void Atualizar()
    {
        for (int i = 0; i < textos.Length; i++)
        {
            if (!botoes[i].interactable) textos[i].color = corBloqueado;
            else textos[i].color = (i == indice) ? corSelecionado : corNormal;
        }

        if (usandoTeclado)
            cursor.PosicionarNosBotao(botoes[indice], distanciaSetaBotao);
    }

    private void Confirmar()
    {
        if (!botoes[indice].interactable) return;

        switch (indice)
        {
            case NOVO_JOGO:
                if (File.Exists(CaminhoSave)) File.Delete(CaminhoSave);
                SceneManager.LoadScene(cenaNovoJogo);
                break;

            case CONTINUAR:
                SceneManager.LoadScene(cenaNovoJogo);
                break;

            case CONFIG:
                painelMenu.SetActive(false);
                painelConfiguracoes.SetActive(true);
                break;

            case SAIR:
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }

    public void FecharConfiguracoes()
    {
        painelConfiguracoes.SetActive(false);
        painelMenu.SetActive(true);
        usandoTeclado = false;
        cursor.VoltarAoMouse();
        Atualizar();
    }

#if ENABLE_INPUT_SYSTEM
    private bool Cima() => Keyboard.current != null && (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame)
                            || Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame;
    private bool Baixo() => Keyboard.current != null && (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
                            || Gamepad.current != null && Gamepad.current.dpad.down.wasPressedThisFrame;
    private bool Confirma() => Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                            || Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
    private bool Voltar() => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame
                            || Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
#else
    private bool Cima()     => Input.GetKeyDown(KeyCode.UpArrow)   || Input.GetKeyDown(KeyCode.W);
    private bool Baixo()    => Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
    private bool Confirma() => Input.GetKeyDown(KeyCode.Return)    || Input.GetKeyDown(KeyCode.Space);
    private bool Voltar()   => Input.GetKeyDown(KeyCode.Escape);
#endif
}