## Map two addresses with ASNA Mobile RPG's DDSGMap control.

This example shows how to use Mobile RPG's Google map control (DDSGMap) to map addresses. In this case, two addresses are mapped, but you can map any number of addresses with Mobile RPG's DDSGMap control. Its map page is shown below. 

![](https://asna.com/filebin/marketing/article-figures/map-two-addresses.png)

The map displayed is a live Google map. You can change its magnification and drap the map pins to change the end points. Although this example shows only two addresses mapped, any number of addresses can be mapped.

The DDSGMap control is very easy to use. [Read its details here.](https://docs.asna.com/documentation/Help150/MonarchFX/_HTML/amfUnderstandingMaps.htm) A quick summary is provided below:

First you need to set a few properties in the DDSGMap control (some of which are shown below). [See this page to read more detail about the DDSGMap's properties.](https://docs.asna.com/documentation/Help150/MonarchFX/_HTML/amfDdsGMapClassMembers.htm)  


![](https://asna.com/filebin/marketing/article-figures/ddsgmap-properties.png?1)


* **AddressField** - The RPG field into which a mapped address is written. This is column in the DDSGMap's subfile. 
* **AddressFieldLength** - The length of the *Char field defined with AddressField.

* **ClearIndicator** - The RPG indicator that clears the file subfile when the control record is written.

* **SubfileControlName** - The name of the subfile controller.

* **SubfileName** - The name of the subfile. 

* **GoogleKey** - This is your organization's Google Map API key. [See the top of this link for more information.](https://docs.asna.com/documentation/Help150/MonarchFX/_HTML/amfUnderstandingMaps.htm) 


The RPG code below is all that's needed to map the two addresses. In this case the two addresses (`AddrTo` and `AddrFm`) are written to the subfile directly. In a production app you'd probably loop over records and write addresses to the DDSGMap subfile in that loop. 

         FMAP       CF   E             WORKSTN Handler('MOBILERPG')
         F                                     SFile(MAPSBF:MAPRRN)
         F                                     Infds(infds)
    
      /copy RPMRDATA/QRPGLESRC,KeyMap

     D MAPRRN          S              4P 0

      /free
        // Set default addresses.
        AddrTo = 'San Antonio, TX';
        AddrFm = 'Marion, IN';
        ExSr ShowMap;
        Exfmt HomeMenu;

        // Exit if F3 is pressed. 
        Dow KeyPressed <> F03;
            // The "Refresh Map" button is mapped to the F10 key.
            If KeyPressed = F10;
                ExSr ShowMap;
            EndIf;
            Exfmt HomeMenu;
        EndDo;

        *INLR = *ON;
        Return;

        BegSr ShowMap;
           // Clear subfile.                
           *IN99 = *ON;
           Write MAPCTRL;

           // Add first address.
           MapRRN = 1;
           Location = AddrFm;
           Write MAPSBF;

           // Add second address. 
           MapRRN = 2;
           Location = AddrTo;
           Write MAPSBF;

           // Write subfile controller.
           *IN99 = *Off;
           Write MAPCTRL;
        EndSr;

