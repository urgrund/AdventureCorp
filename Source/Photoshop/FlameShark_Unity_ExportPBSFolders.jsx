// (c) 2011-2016 Flame Shark
// This script will exports specific layers into TGA's to ease the
// exporting of multi-layer materials into something like a game engine
// Written by Matthew Bell

// PSD Setup
//
// Make sure your PSD has the following layer group/folders
// Albedo                   -   For the diffuse texture
// Alpha                     -  Transparency alpha channel for the diffuse
// Metallic                 -  Metalness value (greyscale) for PBS
// Smoothness         -  Smoothness/Roughness of the PBS shader
// Normal                  -    Normal map
//
// The script will composit the layers as needed so you don't need to
// have Alpha channels in the PSD itself



//#target photoshop
main();

function main()
{
    var strGroupNormal =  "Normal";
    var strNormalExt = "N";

    var strGroupAlpha = "Alpha";
    var strGroupAlbedo = "Albedo";
    var strAlbedoExt = "D";

    var strGroupMetallic = "Metallic";
    var strGroupSmoothness = "Smoothness";
    var strMetalExt = "M";
    

    // Keep reference to the main PSD
    var originalPSD = app.activeDocument;   


    // Get the document path of this PSD    
    var documentPath = "" + app.activeDocument.fullName;    
    var finalSlashPos = documentPath.lastIndexOf('/');
    documentPath = documentPath.substr(0, finalSlashPos+1);


        var newDoc;
        var newName;
        
        // =====================================================================
        // NORMAL MAP 
        newDoc = flattenLayerGroupToDoc (strGroupNormal, strNormalExt);
        newName = getNameFromExt (originalPSD, strNormalExt);
        app.activeDocument = newDoc;           
        saveTGA( app.activeDocument, new File(documentPath+newName), originalPSD, TargaBitsPerPixels.TWENTYFOUR);     
        // =====================================================================



        // =====================================================================
        // ALBEDO
        newDoc = flattenLayerGroupToDoc (strGroupAlbedo, strAlbedoExt);
        newName = getNameFromExt (originalPSD, strAlbedoExt);    
        // Check if alpha and export Albedo as 32bit
        app.activeDocument = originalPSD;    
        var alphaDoc = flattenLayerGroupToDoc (strGroupAlpha, "tmp");
        if(alphaDoc != null)
        {
            app.activeDocument = alphaDoc;
            alphaDoc.selection.selectAll();
            alphaDoc.selection.copy ();       
            alphaDoc.close(SaveOptions.DONOTSAVECHANGES);
            
            app.activeDocument = newDoc;
            var channelRef = app.activeDocument.channels.add();
            newDoc.activeChannels = [app.activeDocument.channels.getByName(channelRef.name)];  
            newDoc.paste ();
            saveTGA( app.activeDocument, new File(documentPath+newName), originalPSD, TargaBitsPerPixels.THIRTYTWO);             
        }
        else
        {
            app.activeDocument = newDoc;
            saveTGA( app.activeDocument, new File(documentPath+newName), originalPSD, TargaBitsPerPixels.TWENTYFOUR);             
        }
        // =====================================================================
    
    
    
    
    
        // =====================================================================
        // METAL & SMOOTHNESS
        newDoc = flattenLayerGroupToDoc (strGroupMetallic, strMetalExt);
        newName = getNameFromExt (originalPSD, strMetalExt);    
        // Check if smoothness exists and export Albedo as 32bit
        app.activeDocument = originalPSD;    
        var alphaDoc = flattenLayerGroupToDoc (strGroupSmoothness, "tmp");
        if(alphaDoc != null)
        {
            app.activeDocument = alphaDoc;
            alphaDoc.selection.selectAll();
            alphaDoc.selection.copy ();       
            alphaDoc.close(SaveOptions.DONOTSAVECHANGES);
            
            app.activeDocument = newDoc;
            var channelRef = app.activeDocument.channels.add();
            newDoc.activeChannels = [app.activeDocument.channels.getByName(channelRef.name)];  
            newDoc.paste ();
            saveTGA( app.activeDocument, new File(documentPath+newName), originalPSD, TargaBitsPerPixels.THIRTYTWO);             
        }
        else
        {
            app.activeDocument = newDoc;
            saveTGA( app.activeDocument, new File(documentPath+newName), originalPSD, TargaBitsPerPixels.TWENTYFOUR);             
        }
        // =====================================================================
}





// -------------------------------------------------------------------
// Builds a string based on the original doc plus the extension
function getNameFromExt(doc, layerExtName)
{
    // Create a new document based on the group name 
    // Make sure to get rid of .PSD in the name 
    //var newName = app.activeDocument.name;
    var newName = doc.name;
    var finalDotPosition = newName.lastIndexOf (".");
    if ( finalDotPosition > -1 ) {
        newName = newName.substr( 0 , finalDotPosition );
    }
    newName +=  "_" + layerExtName;
    return newName;
};





// -------------------------------------------------------------------
// Flattens a layer group to a new document
function flattenLayerGroupToDoc(layerGrpName, layerExtName)
{
     if(layerExist (layerGrpName)){            
            
            var layerSetRef = app.activeDocument.layerSets.getByName(layerGrpName);
            
            // Create a new document based on the group name 
            // Make sure to get rid of .PSD in the name 
            var newName = app.activeDocument.name;
            var finalDotPosition = newName.lastIndexOf (".");
            if ( finalDotPosition > -1 ) {
                    newName = newName.substr( 0 , finalDotPosition );
            }
            newName +=  "_" + layerExtName;
            var originalPSD = app.activeDocument;
            var newDoc = app.documents.add(app.activeDocument.width, app.activeDocument.height, 72, newName, NewDocumentMode.RGB,DocumentFill.TRANSPARENT);
            
            // Duplicate the layer group to the new document
            app.activeDocument =originalPSD;
            layerSetRef.duplicate (app.documents.getByName(newDoc.name));       
            
            // Set active document to be the new doc
            app.activeDocument = newDoc;
            
            // Get the newly duplicated layer
            // merge it all down to a single layer 
            newLayerSet = app.activeDocument.layerSets.getByName(layerGrpName);
            newLayerSet.visible = true;
            var l = newLayerSet.merge();
            l.merge();
            return newDoc;
     }       
 return null;
};





// -------------------------------------------------------------------
// Save out a file as TGA
function saveTGA( doc, saveFile, original, targaBits) {  

     var saveOptions = new TargaSaveOptions();
     saveOptions.resolution = targaBits;
     saveOptions.alphaChannels = true;
     saveOptions.rleCompression=false
     doc.saveAs( saveFile, saveOptions, true );  
      
     // Close up the temp file and return to the main PSD
     doc.close(SaveOptions.DONOTSAVECHANGES);
     app.activeDocument =original;
} ;




// -------------------------------------------------------------------
// Check if a layer exists
function layerExist(lyrName){   
      var desc = new ActionDescriptor();   
      var ref = new ActionReference();   
      ref.putName( charIDToTypeID( "Lyr " ), lyrName);   
      desc.putReference( charIDToTypeID( "null" ), ref );   
      desc.putBoolean( charIDToTypeID( "MkVs" ), false );   
      try{  
      executeAction( charIDToTypeID( "slct" ), desc, DialogModes.NO );   
      }catch(e){return false;}  
      return true;  
}; 