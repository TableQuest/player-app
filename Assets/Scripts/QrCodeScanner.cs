using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QrCodeScanner : MonoBehaviour
{
    private InitialisationClient _initClient;
    [SerializeField]
    private RawImage _rawImageBackground;
    [SerializeField]
    private AspectRatioFitter _aspectRatioFitter;
    [SerializeField]
    private RectTransform _scanZone;
    [SerializeField]
    private TextMeshProUGUI _buttonText;
    private string _qrCodeText;

    private bool _isCamAvailable;
    private WebCamTexture _cameraTexture;

    // Start is called before the first frame update
    void Start()
    {
        _initClient = GameObject.Find("SocketIOClient").GetComponent<InitialisationClient>();
        SetupCamera();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraRender();
        Scan();
    }

    private void SetupCamera()
    {
        WebCamDevice[] _devices = WebCamTexture.devices;

        if (_devices.Length == 0)
        {
            _isCamAvailable = false;
            OnClickSwitchToDev();
            return;
        }

        for (int i = 0; i < _devices.Length; i++)
        {
            if (!_devices[i].isFrontFacing)
            {
                _cameraTexture = new WebCamTexture(_devices[i].name, (int)_scanZone.rect.width, (int)_scanZone.rect.height);
            }
        }

        if (_cameraTexture == null) 
        {
            OnClickSwitchToDev();
            return;
        }
        _cameraTexture.Play();
        _rawImageBackground.texture = _cameraTexture;
        _isCamAvailable = true;
    }

    private void UpdateCameraRender()
    {
        if(!_isCamAvailable)
        {
            return;
        }
        float ratio = (float)_cameraTexture.width / (float)_cameraTexture.height;
        _aspectRatioFitter.aspectRatio = ratio;

        int orientation = -_cameraTexture.videoRotationAngle;
        _rawImageBackground.rectTransform.localEulerAngles = new Vector3(0 ,0 ,orientation);
    }


    private void Scan()
    {
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(_cameraTexture.GetPixels32(), _cameraTexture.width, _cameraTexture.height);
            if(result != null)
            {
                _buttonText.text = result.Text.Split(" ")[1];
                _qrCodeText = result.Text;
            }
            else 
            {
                _buttonText.text = "";
                _qrCodeText = "";
            }
        }
        catch (System.Exception)
        {
            _buttonText.text = "Error";
        }
    }

    public void connectOnClick()
    {
        if(_qrCodeText != "Error" && !(string.IsNullOrEmpty(_qrCodeText)))
        {
            _initClient.SendIdOnClick(_qrCodeText);
        }
    }

    public void OnClickSwitchToDev()
    {
        Destroy(GameObject.Find("SocketIOClient"));
        SceneManager.LoadScene("Connection");
    }
}
