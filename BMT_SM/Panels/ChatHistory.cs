using System.Windows.Forms;

namespace HawkSync_SM
{
    public partial class ChatHistory : Form
    {
        AppState _state;
        public ChatHistory(AppState state)
        {
            InitializeComponent();
            _state = state;
        }
    }
}
