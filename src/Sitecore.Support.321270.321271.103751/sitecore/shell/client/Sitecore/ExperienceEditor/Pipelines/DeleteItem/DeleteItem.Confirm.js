define(["sitecore", "/-/speak/v1/ExperienceEditor/ExperienceEditor.js"], function (Sitecore, ExperienceEditor) {
    
    //commented as WORKAROUND BUG [103751]
    //return ExperienceEditor.PipelinesUtil.generateRequestProcessor("ExperienceEditor.Delete.Confirm");
	
    // WORKAROUND BUG [103751]
	return {
        execute: function () {
            var context = ExperienceEditor.generateDefaultContext();
            return scForm.postRequest("", "", "", 'webedit:delete(id=' + context.currentContext.itemId + ')');
        }
}
});