using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using UnityEngine;

public class SwordData{
    public static int SWING_STATE_NONE = 0;
    public static int SWING_STATE_PRESWING = 1;
    public static int SWING_STATE_SWING = 2;
    public static int SWING_STATE_POSTSWING = 3;

    private int swordId;

    private string swordName;
    private string description;

    private float baseDamage;

    private string imageName;

    private float swordPreSwingDelay;
    private float swingTime;
    private float swordPostSwingDelay;

    private float swingWidth;
    private float swingDistanceFromPlayer;

    public SwordData() {
        // if not given a sword id, default to the default 
        swordId = 0;
        loadProperties();
    }

    public SwordData(int id) {
        swordId = id;

        loadProperties();
    }

    private void loadProperties() {
        XmlDocument swordXml = new XmlDocument();
        swordXml.Load("Assets/data/swords.xml");
        if(swordXml != null) {
            XmlNodeList swordsList = swordXml.GetElementsByTagName("sword");

            // loop through the different swords
            foreach(XmlNode sword in swordsList) {
                int id = int.Parse(sword.Attributes["id"].Value);
                if(id != swordId) {
                    continue;
                }

                // loop through this sword's attributes and set this class' attributes
                foreach(XmlNode swordAttr in sword.ChildNodes) {
                    if(swordAttr.Name == "name") {
                        this.swordName = swordAttr.InnerText.Trim();
                    } else if(swordAttr.Name == "description") {
                        this.description = swordAttr.InnerText.Trim();
                    } else if(swordAttr.Name == "base-damage") {
                        this.baseDamage = float.Parse(swordAttr.InnerText.Trim());
                    } else if(swordAttr.Name == "sprite-image-name") {
                        this.imageName = swordAttr.InnerText.Trim();
                    } else if(swordAttr.Name == "preswing-delay") {
                        this.swordPreSwingDelay = float.Parse(swordAttr.InnerText.Trim());
                    } else if (swordAttr.Name == "swing-time") {
                        this.swingTime = float.Parse(swordAttr.InnerText.Trim());
                    } else if (swordAttr.Name == "postswing-delay") {
                        this.swordPostSwingDelay = float.Parse(swordAttr.InnerText.Trim());
                    } else if (swordAttr.Name == "swing-reach") {
                        this.swingWidth = float.Parse(swordAttr.InnerText.Trim());
                    } else if(swordAttr.Name == "swing-distance-from-player") {
                        this.swingDistanceFromPlayer = float.Parse(swordAttr.InnerText.Trim());
                    }
                }
            }
        } else {
            Debug.LogError("Unable to load sword xml data.  Defaulting to hardcoded values.");
        }
    }

    public float getSwordPreSwingDelay() {
        return this.swordPreSwingDelay;
    }

    public float getSwordSwingTime() {
        return this.swingTime;
    }

    public float getSwordPostSwingDelay() {
        return this.swordPostSwingDelay;
    }

    public float getSwordSwingWidth() {
        return this.swingWidth;
    }

    public float getSwordSwingDistanceFromPlayer() {
        return this.swingDistanceFromPlayer;
    }

    public string getImagePath() {
        string res = "";

        if(imageName != null) {
            res = "Assets/sprites/weapons/" + imageName;
        }

        return res;
    }
}
