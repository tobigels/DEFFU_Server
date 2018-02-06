using System;

[Serializable]
public class Player {

    #region FIELDS

    private int id;
    private string name;

    #endregion

    #region METHODS

    // --------------------------------------- Private methods ---------------------------------------

    // --------------------------------------- Public methods ---------------------------------------

    public Player() {
    }

    public Player(int nId, string nName) {
        id = nId;
        name = nName;
    }

    public int Id {
        get {
            return id;
        }
        set {
            id = value;
        }
    }

    public string Name {
        get {
            return name;
        }
        set {
            name = value;
        }
    }

    #endregion
}
