namespace ABC_Retail_Part1.Models
{
    public class OrderIndexViewModel
    
    {
        public IEnumerable<Order> Orders { get; set; } = new List<Order>();
        public IEnumerable<string> QueueMessages { get; set; } = new List<string>();
    }
}