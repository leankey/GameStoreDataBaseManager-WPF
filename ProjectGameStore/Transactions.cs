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
    
    public partial class Transactions
    {
        public int TransactionID { get; set; }
        public int UserID { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime Date { get; set; }
    
        public virtual Users Users { get; set; }
    }
}
