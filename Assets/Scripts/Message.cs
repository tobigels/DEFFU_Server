using System;

[Serializable]
internal class Message {

    #region FIELDS

    private int type;
    private byte[] content;

    #endregion

    #region METHODS

    // --------------------------------------- Private methods ---------------------------------------

    // --------------------------------------- Public methods ---------------------------------------

    public byte[] Content {
        get {
            return content;
        }
        set {
            content = value;
        }
    }

    public int Type {
        get {
            return type;
        }
        set {
            type = value;
        }
    }

    #endregion
}

[Serializable]
internal class ServerMessage : Message {
    #region FIELDS

    private int origin;

    #endregion

    #region METHODS

    // --------------------------------------- Private methods ---------------------------------------

    // --------------------------------------- Public methods ---------------------------------------

    public int Origin {
        get {
            return origin;
        }
        set {
            origin = value;
        }
    }

    #endregion

}
