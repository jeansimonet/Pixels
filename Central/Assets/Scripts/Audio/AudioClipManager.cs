using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Linq;

public class AudioClipManager : SingletonMonoBehaviour<AudioClipManager>
{
    [Header("Built-in Audio files")]
    public List<AudioClip> builtInClips;

    // The list of clips scanned from the app persistent storage
    public List<AudioClip> userClips = new List<AudioClip>();

    public class AudioClipInfo
    {
        public bool builtIn;
        public AudioClip clip;
        public Texture2D preview;
    }

    public List<AudioClipInfo> audioClips = new List<AudioClipInfo>();

    public AudioClipInfo FindClip(string name)
    {
        return audioClips.FirstOrDefault(a => string.Compare(a.clip.name, name, true) == 0);
    }

    string userClipsRootPath => Path.Combine(Application.persistentDataPath, AppConstants.Instance.AudioClipsFolderName);

    class AudioFileImportInfo
    {
        public string fileName;
        public string filePath;
        public AudioType type;
    }

    void Start()
    {
        if (!Directory.Exists(userClipsRootPath))
        {
            Directory.CreateDirectory(userClipsRootPath);
        }

        StartCoroutine(LoadUserFiles());
    }

    IEnumerator LoadUserFiles()
    {
        if (Directory.Exists(userClipsRootPath))
        {
            var audioFileInfos = new List<AudioFileImportInfo>();
            DirectoryInfo info = new DirectoryInfo(userClipsRootPath);
            foreach (FileInfo item in info.GetFiles())
            {
                if (item.Extension == ".wav")
                {
                    audioFileInfos.Add(new AudioFileImportInfo()
                        {
                            fileName = item.Name,
                            filePath = userClipsRootPath + "/" + item.Name,
                            type = AudioType.WAV
                        });
                }
                // else if (item.Extension == ".mp3")
                // {
                //     audioFileInfos.Add(new AudioFileInfo()
                //         {
                //             fileName = item.Name,
                //             filePath = userClipsRootPath + "/" + item.Name,
                //             type = AudioType.MPEG
                //         });
                // }
            }

            foreach (var audioFileInfo in audioFileInfos)
            {
                UnityWebRequest AudioFileRequest = UnityWebRequestMultimedia.GetAudioClip(audioFileInfo.filePath, audioFileInfo.type);
                yield return AudioFileRequest.SendWebRequest();
                if (!AudioFileRequest.isNetworkError)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(AudioFileRequest);
                    clip.name = audioFileInfo.fileName;
                    userClips.Add(clip);
                    Debug.Log("Imported user audio clip: " + audioFileInfo.filePath);
                }
            }
        }

        // Generate previews for all clips
        for (int i = 0; i < builtInClips.Count; ++i)
        {
            var clip = builtInClips[i];
            var preview = AudioUtils.PaintWaveformSpectrum(clip, 0.7f, 256, 256, AppConstants.Instance.AudioClipsWaveformColor);
            var hash = AudioUtils.GenerateWaveformHash(clip);
            audioClips.Add(new AudioClipInfo() {
                builtIn = true,
                clip = clip,
                preview = preview });

            // Wait until next frame to continue
            yield return null;
        }

        for (int i = 0; i < userClips.Count; ++i)
        {
            var clip = userClips[i];
            var preview = AudioUtils.PaintWaveformSpectrum(clip, 0.7f, 256, 256, AppConstants.Instance.AudioClipsWaveformColor);
            var hash = AudioUtils.GenerateWaveformHash(clip);
            audioClips.Add(new AudioClipInfo() {
                builtIn = false,
                clip = clip,
                preview = preview });

            // Wait until next frame to continue
            yield return null;
        }
    }
}
