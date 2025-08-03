using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory_Management_System_SFML
{
    public class salesOrderViewModel : INotifyPropertyChanged
    {
        private DateTime? _orderDate;

        public DateTime? OrderDate
        {
            get => _orderDate;
            set
            {
                if (_orderDate != value)
                {
                    _orderDate = value;
                    OnPropertyChanged(nameof(OrderDate));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
