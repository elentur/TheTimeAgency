

    public class GameStateMessage
    {
        public bool win = false;
        public string message = "";

        public GameStateMessage(bool win, string message)
        {
            this.win = win;
            this.message = message;
        }
        public GameStateMessage( string message)
        {
            this.message = message;
           
        }
    }

