using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

// Source: https://doc.photonengine.com/en-us/pun/v2/demos-and-tutorials/oculusavatarsdk
namespace Com.ATL3Y.Test
{
    public class NetworkManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public void OnEvent ( EventData photonEvent )
        {
            if ( photonEvent.Code == InstantiateVrAvatarEventCode )
            {
                GameObject remoteAvatar = Instantiate(Resources.Load("RemoteAvatar")) as GameObject;
                PhotonView photonView = remoteAvatar.GetComponent<PhotonView>();
                photonView.ViewID = ( int ) photonEvent.CustomData;
            }
        }

        //  register our previously implemented OnEvent callback
        public void OnEnable ( )
        {
            PhotonNetwork.AddCallbackTarget ( this );
        }

        public void OnDisable ( )
        {
            PhotonNetwork.RemoveCallbackTarget ( this );
        }

        #region MonoBehaviourPunCallbacks Callbacks

        public readonly byte InstantiateVrAvatarEventCode = 123;

        // The following is specific to OVR integration, which requires manual instantiation 
        // due to its use of the local and remote avatars. 
        // Otherwise we could simply use PhotonNetwork.Instantiate
        public override void OnJoinedRoom ( )
        {
            GameObject localAvatar = Instantiate(Resources.Load("LocalAvatar")) as GameObject;
            PhotonView photonView = localAvatar.GetComponent<PhotonView>();

            if ( PhotonNetwork.AllocateViewID ( photonView ) )
            {
                // Instantiate our local camera
                // Note: stagger spawn pos based on player count. 
                // Note: give each player a color.
                //PhotonNetwork.Instantiate ( "OVRCameraRig", Vector3.zero, Quaternion.identity, 0, null );

                // Tell the other games to instantiate our remote avatar
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                {
                    CachingOption = EventCaching.AddToRoomCache,
                    Receivers = ReceiverGroup.Others
                };

                SendOptions sendOptions = new SendOptions
                {
                    Reliability = true
                };

                PhotonNetwork.RaiseEvent ( InstantiateVrAvatarEventCode, photonView.ViewID, raiseEventOptions, sendOptions );
            }
            else
            {
                Debug.LogError ( "Failed to allocate a ViewId." );

                Destroy ( localAvatar );
            }
        }

        #endregion
    }
}
