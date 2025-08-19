
using if_hub.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Topico> Topicos { get; set; }
    public DbSet<Resposta> Respostas { get; set; }
    public DbSet<Curtida> Curtidas { get; set; }
    public DbSet<Notificacao> Notificacoes { get; set; }
    public DbSet<LogAcao> LogAcoes { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TopicoTag> TopicoTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Curtida Configuration (for relationships) ---
        modelBuilder.Entity<Curtida>(entity =>
        {
            entity.HasOne(c => c.Usuario)
                  .WithMany(u => u.Curtidas)
                  .HasForeignKey(c => c.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Topico)
                  .WithMany(t => t.Curtidas)
                  .HasForeignKey(c => c.TopicoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.Resposta)
                  .WithMany(r => r.Curtidas)
                  .HasForeignKey(c => c.RespostaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Usuario Configuration ---
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.SenhaHash).IsRequired();
            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.Banido).HasDefaultValue(false);

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.Usuarios)
                  .HasForeignKey(e => e.RoleId);
        });

        // --- Resposta Configuration (for self-referencing relationship) ---
        modelBuilder.Entity<Resposta>(entity =>
        {
            entity.HasOne(e => e.RespostaPai)
                  .WithMany(e => e.RespostasFilhas)
                  .HasForeignKey(e => e.RespostaPaiId)
                  .OnDelete(DeleteBehavior.Restrict); 
        });

        // --- TopicoTag Configuration (Many-to-Many Composite Key) ---
        modelBuilder.Entity<TopicoTag>(entity =>
        {
            entity.HasKey(tt => new { tt.TopicoId, tt.TagId }); 

            entity.HasOne(tt => tt.Topico)
                  .WithMany(t => t.TopicoTags)
                  .HasForeignKey(tt => tt.TopicoId);

            entity.HasOne(tt => tt.Tag)
                  .WithMany(t => t.TopicoTags)
                  .HasForeignKey(tt => tt.TagId);
        });

        var seedDate = new DateTime(2025, 6, 22, 20, 30, 0, DateTimeKind.Utc);

        // 1. Popular a tabela de Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Nome = "Aluno" },
            new Role { Id = 2, Nome = "Moderador" },
            new Role { Id = 3, Nome = "Admin" }
        );

        // 2. Popular a tabela de Categorias
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nome = "Programação", Descricao = "Discussões sobre linguagens, algoritmos e desenvolvimento de software.", DataCriacao = seedDate },
            new Categoria { Id = 2, Nome = "Cálculo", Descricao = "Tópicos sobre limites, derivadas, integrais e aplicações.", DataCriacao = seedDate },
            new Categoria { Id = 3, Nome = "Hardware", Descricao = "Discussões sobre componentes de computador, montagem e manutenção.", DataCriacao = seedDate }
        );

        // 3. Popular com Usuários Iniciais        
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nome = "Admin User",
                Email = "admin@email.com",
                SenhaHash = "AQAAAAIAAYagAAAAEPhH/65zZtC4N9YtCqY9iE2n5xZ3zZ+yZ+e8w3c9X6b8V5n7f7j3X6n8Y7d9V6c3",
                DataCriacao = seedDate,
                Ativo = true,
                Banido = false,
                RoleId = 3 
            },
            new Usuario
            {
                Id = 2,
                Nome = "Aluno Teste",
                Email = "aluno@email.com",
                SenhaHash = "AQAAAAIAAYagAAAAEJ5y/8f6zZtC4N9YtCqY9iE2n5xZ3zZ+yZ+e8w3c9X6b8V5n7f7j3X6n8Y7d9V6c4",
                DataCriacao = seedDate,
                Ativo = true,
                Banido = false,
                RoleId = 1 
            }
        );

        // 4. Popular com um Tópico Inicial
        modelBuilder.Entity<Topico>().HasData(
            new Topico
            {
                Id = 1,
                Titulo = "Dúvida sobre ponteiros em C++",
                Conteudo = "Olá pessoal, estou com dificuldade para entender como funcionam os ponteiros para ponteiros em C++. Alguém poderia me dar uma luz?",
                DataCriacao = seedDate,
                UsuarioId = 2, 
                CategoriaId = 1 
            }
        );

        // 5. Popular com uma Resposta Inicial
        modelBuilder.Entity<Resposta>().HasData(
            new Resposta
            {
                Id = 1,
                Conteudo = "Claro! Um ponteiro para ponteiro armazena o endereço de memória de outro ponteiro. Pense nele como um 'índice' para seus 'índices' de memória.",
                DataCriacao = seedDate,
                UsuarioId = 1, 
                TopicoId = 1,
                RespostaPaiId = null 
            }
        );
    }
}