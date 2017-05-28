using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using UnityEngine;

public class ShieldData {
    private int shieldId;

    private string shieldName;
    private string description;
    private float shieldDistanceFromPlayer = 1f;

    public ShieldData() {
        // if not given a sword id, default to the default 
        shieldId = 0;
        loadProperties();
    }

    public ShieldData(int id) {
        shieldId = id;

        loadProperties();
    }

    private void loadProperties() {
        XmlDocument shieldXml = new XmlDocument();
        shieldXml.Load("Assets/data/shields.xml");
        if(shieldXml != null) {
            XmlNodeList shieldsList = shieldXml.GetElementsByTagName("shield");

            // loop through the different swords
            foreach(XmlNode shield in shieldsList) {
                int id = int.Parse(shield.Attributes["id"].Value);
                if(id != shieldId) {
                    continue;
                }

                // loop through this sword's attributes and set this class' attributes
                foreach(XmlNode shieldAttr in shield.ChildNodes) {
                    if(shieldAttr.Name == "name") {
                        this.shieldName = shieldAttr.InnerText.Trim();
                    } else if(shieldAttr.Name == "description") {
                        this.description = shieldAttr.InnerText.Trim();
                    } else if(shieldAttr.Name == "distance-from-player") {
                        this.shieldDistanceFromPlayer = float.Parse(shieldAttr.InnerText.Trim());
                    }
                }
            }
        } else {
            Debug.LogError("Unable to load shield xml data.  Defaulting to hardcoded values.");
        }
    }



    public string getName() {
        return this.shieldName;
    }

    public string getDescription() {
        return this.description;
    }

    public float getDistanceFromPlayer() {
        return this.shieldDistanceFromPlayer;
    }
}
