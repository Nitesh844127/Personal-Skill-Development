namespace skillDev.Models
{
    public class Receipt
    {
        public int id { get; set; }
        public int date { get; set; } 
        public int studId { get; set; } 
        public decimal amount { get; set; } 
        public string remark { get; set; }
        public string studName { get; set; }
        public string courseName { get; set; }
        public string className { get; set;}
        public string mob { get; set;}


    }
}

