using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SerializationUnit {

    // --------------------------------------- Private methods ---------------------------------------

    // --------------------------------------- Public methods ---------------------------------------

    /// <summary>
    /// 
    /// </summary>
    /// <param name="toSerialize"></param>
    /// <returns></returns>
    public byte[] SerializeHelper(object toSerialize) {
        var memoryStream = new MemoryStream();
        byte[] returnValue;
        try {
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            binaryFormatter.Serialize(memoryStream, toSerialize);
            returnValue = memoryStream.ToArray();
        } catch (SerializationException e) {
            Debug.Log("Error: failed to deserialize with reason " + e);
            throw;
        } finally {
            memoryStream.Close();
        }

        return returnValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="bufferLength"></param>
    /// <returns></returns>
    public object DeserializeHelper(byte[] buffer, int bufferLength) {
        var memoryStream = new MemoryStream(buffer, 0, bufferLength);
        object returnValue;
        try {
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            returnValue = binaryFormatter.Deserialize(memoryStream);
        } catch (SerializationException e) {
            Debug.Log("Error: failed to deserialize with reason " + e);
            throw;
        } finally {
            memoryStream.Close();
        }

        return returnValue;
    }
}