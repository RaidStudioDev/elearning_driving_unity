using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SSLAuth : CertificateHandler
{
    // fake auth certificate
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Debug.Log("ValidateCertificate");
        return true;
    }
}
