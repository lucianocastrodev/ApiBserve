using System.ComponentModel.DataAnnotations.Schema;

namespace ApiBserve.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }

        [Column("preco_compra")]
        public decimal PrecoCompra { get; set; }

        [Column("preco_venda")]
        public decimal PrecoVenda { get; set; }

        public int Estoque { get; set; }

        [Column("codigo_barras")]
        public string? CodigoBarras { get; set; }
    }
}