mergeInto(LibraryManager.library, 
{
  IsSafari: function () 
  {
  	return /^((?!chrome|android).)*safari/i.test(navigator.userAgent);
  }
});