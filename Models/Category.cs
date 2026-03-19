using System.ComponentModel.DataAnnotations;

namespace WEBDULICH.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        // 1 category có thể có nhiều Destinations , nếu như bảng database là Destinations thì trong phần services giữ tương tự để ánh xạ đến các bảng csdl và ko liên quan đến cái Destiionation này ...
        
        public ICollection<Destination> Destinations { get; set; }
    }

}
