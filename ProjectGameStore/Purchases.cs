//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProjectGameStore
{
    using System;
    using System.Collections.Generic;
    
    public partial class Purchases
    {
        public int PurchaseID { get; set; }
        public int UserID { get; set; }
        public int GameID { get; set; }
        public System.DateTime PurchaseDate { get; set; }
    
        public virtual Games Games { get; set; }
        public virtual Users Users { get; set; }
    }
}