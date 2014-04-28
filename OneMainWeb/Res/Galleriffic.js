/*
 * jQuery Galleriffic plugin
 *
 * Copyright (c) 2008 Trent Foley
 * Licensed under the MIT License:
 *   http://www.opensource.org/licenses/mit-license.php
 *
 * Thanks to Taku Sano (Mikage Sawatari), whose history plugin I adapted to work with Galleriffic
 */
;(function($) {

	// Write noscript style
	document.write("<style type='text/css'>.noscript{display:none}</style>");

	var ver = 'galleriffic-0.7';
	var galleryOffset = 0;
	var galleries = [];
	var allImages = [];	
	var historyCurrentHash;
	var historyBackStack;
	var historyForwardStack;
	var isFirst = false;
	var dontCheck = false;
	var isInitialized = false;

	function getHash() {
		var hash = location.hash;
		if (!hash) return -1;
		hash = hash.replace(/^.*#/, '');
		if (isNaN(hash)) return -1;
		return (+hash);
	}

	function registerGallery(gallery) {
		galleries.push(gallery);
		
		// update the global offset value
		galleryOffset += gallery.data.length;
	}

	function getGallery(hash) {
		for (i = 0; i < galleries.length; i++) {
			var gallery = galleries[i];
			if (hash < (gallery.data.length+gallery.offset))
				return gallery;
		}
		return 0;
	}
	
	function historyCallback() {
		// Using present location.hash always (seems to work, unlike the hash argument passed to this callback)
		var hash = getHash();
		if (hash < 0) return;

		var gallery = getGallery(hash);
		if (!gallery) return;
		
		var index = hash-gallery.offset;
		gallery.goto(index);
	}
	
	function historyInit() {
		if (isInitialized) return;
		isInitialized = true; 

		var current_hash = location.hash;
		
		historyCurrentHash = current_hash;
		if ($.browser.msie) {
			// To stop the callback firing twice during initilization if no hash present
			if (historyCurrentHash == '') {
				historyCurrentHash = '#';
			}
		} else if ($.browser.safari) {
			// etablish back/forward stacks
			historyBackStack = [];
			historyBackStack.length = history.length;
			historyForwardStack = [];
			isFirst = true;
		}

		setInterval(function() { historyCheck(); }, 100);
	}
	
	function historyAddHistory(hash) {
		// This makes the looping function do something
		historyBackStack.push(hash);
		historyForwardStack.length = 0; // clear forwardStack (true click occured)
		isFirst = true;
	}
	
	function historyCheck() {
		if ($.browser.safari) {
			if (!dontCheck) {
				var historyDelta = history.length - historyBackStack.length;
				
				if (historyDelta) { // back or forward button has been pushed
					isFirst = false;
					if (historyDelta < 0) { // back button has been pushed
						// move items to forward stack
						for (var i = 0; i < Math.abs(historyDelta); i++) historyForwardStack.unshift(historyBackStack.pop());
					} else { // forward button has been pushed
						// move items to back stack
						for (var i = 0; i < historyDelta; i++) historyBackStack.push(historyForwardStack.shift());
					}
					var cachedHash = historyBackStack[historyBackStack.length - 1];
					if (cachedHash != undefined) {
						historyCurrentHash = location.hash;
						historyCallback();
					}
				} else if (historyBackStack[historyBackStack.length - 1] == undefined && !isFirst) {
					historyCallback();
					isFirst = true;
				}
			}
		} else {
			// otherwise, check for location.hash
			var current_hash = location.hash;
			if(current_hash != historyCurrentHash) {
				historyCurrentHash = current_hash;
				historyCallback();
			}
		}
	}

	var defaults = {
		delay:                3000,
		numThumbs:            20,
		preloadAhead:         40, // Set to -1 to preload all images
		enableTopPager:       true,
		enableBottomPager:    true,
		imageContainerSel:    '',
		thumbsContainerSel:   '',
		controlsContainerSel: '',
		titleContainerSel:    '',
		descContainerSel:     '',
		downloadLinkSel:      '',
		renderSSControls:     true,
		renderNavControls:    true,
		playLinkText:         'Play',
		pauseLinkText:        'Pause',
		prevLinkText:         'Previous',
		nextLinkText:         'Next',
		nextPageLinkText:     'Next &rsaquo;',
		prevPageLinkText:     '&lsaquo; Prev'
	};

	function clickHandler(gallery) {
		gallery.pause();
		return false;
	}

	$.fn.galleriffic = function(thumbsContainerSel, settings) {
		//  Extend Gallery Object
		$.extend(this, {
			ver: function() {
				return ver;
			},

			buildDataFromThumbs: function() {
				this.data = [];
				var gallery = this;
				this.$thumbsContainer.find('li').each(function(i) {
					var $a = $(this).find('a');
					var $img = $a.find('img:first');
					gallery.data.push({slide:$a.attr('href'),thumb:$img.attr('src'),original:$a.attr('original'),title:$a.attr('title'),description:$a.attr('description'),hash:gallery.offset+i});
				});
				return this;
			},

			isPreloadComplete: false,

			preloadInit: function() {
				if (this.settings.preloadAhead == 0) return this;
				
				this.preloadStartIndex = this.currentIndex;
				var nextIndex = this.getNextIndex(this.preloadStartIndex);
				return this.preloadRecursive(this.preloadStartIndex, nextIndex);
			},
			
			preloadRelocate: function(index) {
				// By changing this startIndex, the current preload script will restart
				this.preloadStartIndex = index;
				return this;
			},

			preloadRecursive: function(startIndex, currentIndex) {
				// Check if startIndex has been relocated
				if (startIndex != this.preloadStartIndex) {
					var nextIndex = this.getNextIndex(this.preloadStartIndex);
					return this.preloadRecursive(this.preloadStartIndex, nextIndex);
				}

				var gallery = this;

				// Now check for preloadAhead count
				var preloadCount = currentIndex - startIndex;
				if (preloadCount < 0)
					preloadCount = this.data.length-1-startIndex+currentIndex;
				if (this.settings.preloadAhead >= 0 && preloadCount > this.settings.preloadAhead) {
					// Do this in order to keep checking for relocated start index
					setTimeout(function() { gallery.preloadRecursive(startIndex, currentIndex); }, 500);
					return this;
				}

				var imageData = this.data[currentIndex];

				// If already loaded, continue
				if (imageData.$image)
					return this.preloadNext(startIndex, currentIndex); 
				
				// Preload the image
				var image = new Image();
				
				image.onload = function() {
					imageData.$image = this;
					gallery.preloadNext(startIndex, currentIndex);
				};

				image.alt = imageData.title;
				image.src = imageData.slide;

				return this;
			},
			
			preloadNext: function(startIndex, currentIndex) {
				var nextIndex = this.getNextIndex(currentIndex);
				if (nextIndex == startIndex) {
					this.isPreloadComplete = true;
				} else {
					// Use set timeout to free up thread
					var gallery = this;
					setTimeout(function() { gallery.preloadRecursive(startIndex, nextIndex); }, 100);
				}
				return this;
			},

			getNextIndex: function(index) {
				var nextIndex = index+1;
				if (nextIndex >= this.data.length)
					nextIndex = 0;
				return nextIndex;
			},
			
			getPrevIndex: function(index) {
				var prevIndex = index-1;
				if (prevIndex < 0)
					prevIndex = this.data.length-1;
				return prevIndex;
			},

			pause: function() {
				if (this.interval)
					this.toggleSlideshow();
				
				return this;
			},

			play: function() {
				if (!this.interval)
					this.toggleSlideshow();
				
				return this;
			},

			toggleSlideshow: function() {
				if (this.interval) {
					clearInterval(this.interval);
					this.interval = 0;
					
					if (this.$controlsContainer) {
						this.$controlsContainer
							.find('div.ss-controls span').removeClass().addClass('play')
							.attr('title', this.settings.playLinkText)
							.html(this.settings.playLinkText);
					}
				} else {
					this.ssAdvance();

					var gallery = this;
					this.interval = setInterval(function() {
						gallery.ssAdvance();
					}, this.settings.delay);
					
					if (this.$controlsContainer) {
						this.$controlsContainer
							.find('div.ss-controls span').removeClass().addClass('pause')
							.attr('title', this.settings.pauseLinkText)
							.html(this.settings.pauseLinkText);
					}
				}

				return this;
			},

			ssAdvance: function() {
				var nextIndex = this.getNextIndex(this.currentIndex);
				var nextHash = this.data[nextIndex].hash;
				
				// Seems to be working on both FF and Safari
				location.href = '#'+nextHash;

				// IE we need to explicity call goto
				if ($.browser.msie) {
					this.goto(nextIndex);
				}

				return this;
			},

			goto: function(index) {
				if (index < 0) index = 0;
				else if (index >= this.data.length) index = this.data.length-1;
				this.currentIndex = index;
				this.preloadRelocate(index);
				return this.refresh();
			},
			
			refresh: function() {
				if (this.$imageContainer) {
					var imageData = this.data[this.currentIndex];
					var isFading = 1;
					var gallery = this;

					// hide img
					this.$imageContainer.fadeOut('fast', function() {
						isFading = 0;

						// Update Controls
						if (gallery.$controlsContainer) {
							gallery.$controlsContainer
								.find('div.nav-controls a.prev').attr('href', '#'+gallery.data[gallery.getPrevIndex(gallery.currentIndex)].hash).end()
								.find('div.nav-controls a.next').attr('href', '#'+gallery.data[gallery.getNextIndex(gallery.currentIndex)].hash);
						}

						// Replace Title
						if (gallery.$titleContainer) {
							gallery.$titleContainer.empty().html(imageData.title);
						}

						// Replace Description
						if (gallery.$descContainer) {
							gallery.$descContainer.empty().html(imageData.description);
						}

						// Update Download Link
						if (gallery.$downloadLink) {
							gallery.$downloadLink.attr('href', imageData.original);
						}

						if (imageData.$image) {
							gallery.buildImage(imageData.$image);
						}
					});
					
					if (this.onFadeOut) this.onFadeOut();

					if (!imageData.$image) {
						var image = new Image();
						// Wire up mainImage onload event
						image.onload = function() {
							imageData.$image = this;

							if (!isFading) {
								gallery.buildImage(imageData.$image);
							}
						};

						// set alt and src
						image.alt = imageData.title;
						image.src = imageData.slide;
					}

					// This causes the preloader (if still running) to relocate out from the currentIndex
					this.relocatePreload = true;
				}

				return this.syncThumbs();
			},
			
			buildImage: function(image) {
				if (this.$imageContainer) {
					this.$imageContainer.empty();

					var gallery = this;

					// Setup image
					this.$imageContainer
						.append('<span class="image-wrapper"><a class="advance-link" rel="history" href="#'+this.data[this.getNextIndex(this.currentIndex)].hash+'" title="'+image.alt+'"></a></span>')
						.find('a')
						.append(image)
						.click(function() { clickHandler(gallery); })
						.end()
						.fadeIn('fast');
					
					if (this.onFadeIn) this.onFadeIn();
				}

				return this;
			},

			syncThumbs: function() {
		        if (this.$thumbsContainer) {
					var page = Math.floor(this.currentIndex / this.settings.numThumbs);
			        if (page != this.currentPage) {
			            this.currentPage = page;
			            this.updateThumbs();
					} else {
						var selectedThumb = this.currentIndex % this.settings.numThumbs;

						// Remove existing selected class and add selected class to new thumb
						this.$thumbsContainer
							.find('ul.thumbs li.selected')
							.removeClass('selected')
							.end()
							//.find('ul.thumbs a[href="#'+this.currentIndex+'"]')
							.find('ul.thumbs li').eq(selectedThumb)
							.addClass('selected');
					}
				}

				return this;
			},

			updateThumbs: function() {
				if (this.$thumbsContainer) {
					// Initialize currentPage to first page
					if (this.currentPage < 0)
						this.currentPage = 0;
				
					var startIndex = this.currentPage*this.settings.numThumbs;
			        var stopIndex = startIndex+this.settings.numThumbs-1;
			        if (stopIndex >= this.data.length)
						stopIndex = this.data.length-1;

					var needsPagination = this.data.length > this.settings.numThumbs;

					// Clear thumbs container
					this.$thumbsContainer.empty();
				
					// Rebuild top pager
					this.$thumbsContainer.append('<div class="top pagination"></div>');
					if (needsPagination && this.settings.enableTopPager) {
						this.buildPager(this.$thumbsContainer.find('div.top'));
					}

					// Rebuild thumbs
					var $ulThumbs = this.$thumbsContainer.append('<ul class="thumbs"></ul>').find('ul.thumbs');
					for (i=startIndex; i<=stopIndex; i++) {
						var selected = '';
					
						if (i==this.currentIndex)
							selected = ' class="selected"';
						
						var imageData = this.data[i];
						$ulThumbs.append('<li'+selected+'><a rel="history" href="#'+imageData.hash+'" title="'+imageData.title+'"><img src="'+imageData.thumb+'" alt="'+imageData.title+'" /></a></li>');
					}

					// Rebuild bottom pager
					if (needsPagination && this.settings.enableBottomPager) {
						this.$thumbsContainer.append('<div class="bottom pagination"></div>');
						this.buildPager(this.$thumbsContainer.find('div.bottom'));
					}

					// Add click handlers
					var gallery = this;
					this.$thumbsContainer.find('a[rel="history"]').click(function() { clickHandler(gallery); });
				}

				return this;
			},

			buildPager: function(pager) {
				var startIndex = this.currentPage*this.settings.numThumbs;
				
				// Prev Page Link
				if (this.currentPage > 0) {
					var prevPage = startIndex - this.settings.numThumbs;
					pager.append('<a rel="history" href="#'+this.data[prevPage].hash+'" title="'+this.settings.prevPageLinkText+'">'+this.settings.prevPageLinkText+'</a>');
				}
				
				// Page Index Links
				for (i=this.currentPage-3; i<=this.currentPage+3; i++) {
					var pageNum = i+1;
					
					if (i == this.currentPage)
						pager.append('<strong>'+pageNum+'</strong>');
					else {
						var imageIndex = i*this.settings.numThumbs;
						if (i>=0 && i<this.numPages) {
							pager.append('<a rel="history" href="#'+this.data[imageIndex].hash+'" title="'+pageNum+'">'+pageNum+'</a>');
						}
					}
				}
				
				// Next Page Link
				var nextPage = startIndex+this.settings.numThumbs;
				if (nextPage < this.data.length) {
					pager.append('<a rel="history" href="#'+this.data[nextPage].hash+'" title="'+this.settings.nextPageLinkText+'">'+this.settings.nextPageLinkText+'</a>');
				}
				
				return this;
			}
		});

		// Now initialize the gallery
		this.settings = $.extend({}, defaults, settings);

		if (this.interval)
			clearInterval(this.interval);

		this.interval = 0;
		
		if (this.settings.imageContainerSel) this.$imageContainer = $(this.settings.imageContainerSel);
		if (this.settings.thumbsContainerSel) this.$thumbsContainer = $(this.settings.thumbsContainerSel);
		if (this.settings.titleContainerSel) this.$titleContainer = $(this.settings.titleContainerSel);
		if (this.settings.descContainerSel) this.$descContainer = $(this.settings.descContainerSel);
		if (this.settings.downloadLinkSel) this.$downloadLink = $(this.settings.downloadLinkSel);

		// Set the hash index offset for this gallery
		this.offset = galleryOffset;

		// This is for backward compatibility
		if (thumbsContainerSel instanceof Array) {
			this.data = thumbsContainerSel;
		} else {
			this.$thumbsContainer = $(thumbsContainerSel);
			this.buildDataFromThumbs();
		}
		
		// Add this gallery to the global galleries array
		registerGallery(this);

		this.numPages = Math.ceil(this.data.length/this.settings.numThumbs);
		this.currentPage = -1;
		this.currentIndex = 0;
		var gallery = this;

		// Setup controls
		if (this.settings.controlsContainerSel) {
			this.$controlsContainer = $(this.settings.controlsContainerSel).empty();
			
			if (this.settings.renderSSControls) {
				this.$controlsContainer
					.append('<div class="ss-controls"><span class="play" title="'+this.settings.playLinkText+'">'+this.settings.playLinkText+'</span></div>')
					.find('div.ss-controls span')
					.click(function() { gallery.toggleSlideshow(); });
			}
		
			if (this.settings.renderNavControls) {					
				this.$controlsContainer
					.append('<div class="nav-controls"><a class="prev" rel="history" title="'+this.settings.prevLinkText+'">'+this.settings.prevLinkText+'</a><a class="next" rel="history" title="Next">'+this.settings.nextLinkText+'</a></div>')
					.find('a[rel="history"]')
					.click(function() { clickHandler(gallery); });
			}
		}

		// Initialize history only once when the first gallery on the page is initialized
		historyInit();

		// Build image
		var hash = getHash();
		var hashGallery = (hash >= 0) ? getGallery(hash) : 0;
		var gotoIndex = (hashGallery && this == hashGallery) ? (hash-this.offset) : 0;
		this.goto(gotoIndex);

		// Kickoff Image Preloader after 1 second
		setTimeout(function() { gallery.preloadInit(); }, 1000);
		
		return this;
	};

})(jQuery);
