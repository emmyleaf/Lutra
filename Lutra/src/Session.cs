using Lutra.Input;

namespace Lutra
{
    /// <summary>
    /// Class that represents a player session. Use this for maintaining and using information about a player.
    /// For example a two player game might have two sessions, one for each player, each with their own controls
    /// configured and save data.
    /// </summary>
    public class Session
    {

        static private int nextSessionId = 0;

        /// <summary>
        /// The name of this Session. This is important as it will determine the name of save data
        /// files and you can also find a session by name.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// The Controller to use for this Session.
        /// </summary>
        public VirtualController Controller = new();

        /// <summary>
        /// Gets the Controller as a specific type of Controller.
        /// </summary>
        /// <typeparam name="T">The type of Controller.</typeparam>
        /// <returns>The Controller as type T.</returns>
        public T GetController<T>() where T : VirtualController
        {
            return Controller as T;
        }

        /// <summary>
        /// The Id of this session in the Game.
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// The game that manages this session.
        /// </summary>
        public Game Game { get; internal set; }

        /// <summary>
        /// Create a new Session.
        /// </summary>
        /// <param name="game">The Game that the session is tied to.</param>
        public Session(Game game, string name)
        {
            Game = game;
            Name = name;

            Id = nextSessionId;
            nextSessionId++;

            InputManager.AddVirtualController(Controller);
        }
    }
}
