
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class MovimentoCavaleiro : MonoBehaviour
{
    [Header("Movimentacao")]
    public float velocidade = 5f;
    public float forcaPulo = 12f;

    [Header("Componentes")]
    public Rigidbody2D rb;
    public BoxCollider2D colisor;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Chao")]
    public LayerMask camadaChao;

    private float movimentoHorizontal;
    private bool noChao;
    private bool puloSolicitado;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (colisor == null)
            colisor = GetComponent<BoxCollider2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // Movimentacao A e D
        movimentoHorizontal = 0f;

        if (Input.GetKey(KeyCode.A))
            movimentoHorizontal = -1f;

        if (Input.GetKey(KeyCode.D))
            movimentoHorizontal = 1f;

        // Solicitar pulo
        if (Input.GetKeyDown(KeyCode.Space) && noChao)
        {
            puloSolicitado = true;
        }

        // Virar o personagem
        if (movimentoHorizontal != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = movimentoHorizontal < 0;
        }

        // Atualizar animacoes
        AtualizarAnimacoes();
    }

    void FixedUpdate()
    {
        noChao = VerificarChao();

        // Movimentacao fisica
        rb.linearVelocity = new Vector2(
            movimentoHorizontal * velocidade,
            rb.linearVelocity.y
        );

        // Pulo
        if (puloSolicitado && noChao)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                forcaPulo
            );

            noChao = false;
        }

        puloSolicitado = false;
    }

    bool VerificarChao()
    {
        Bounds limites = colisor.bounds;

        Vector2 tamanho = new Vector2(
            limites.size.x * 0.85f,
            0.08f
        );

        Vector2 centro = new Vector2(
            limites.center.x,
            limites.min.y - 0.02f
        );

        return Physics2D.OverlapBox(
            centro,
            tamanho,
            0f,
            camadaChao
        ) != null;
    }

    void AtualizarAnimacoes()
    {
        if (animator == null)
            return;

        bool andando = Mathf.Abs(movimentoHorizontal) > 0.01f;

        animator.SetBool("Andando", andando && noChao);
        animator.SetBool("NoChao", noChao);

        animator.SetFloat("VelocidadeY", rb.linearVelocity.y);
    }
}

