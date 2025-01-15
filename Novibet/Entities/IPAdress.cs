public class IPAddress
{
    public int Id { get; set; }
    public int CountryId { get; set; }  // Chave estrangeira para Countries
    public string IP { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Country Country { get; set; }  // Relacionamento com Countries
}
