<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ArticleSingle.ascx.cs" Inherits="OneMainWeb.CommonModules.ArticleSingle" %>
<section class="mi article mi1">	
		<header class="mi-header">
			<h1>Single Article</h1>
		</header>
		<article class="single hentry">
			<header>
				<hgroup>
					 <h1 class="entry-title">Hold on to your butts</h1>
					 <h2 class="entry-subtitle">We happy?</h2>
				</hgroup>
			</header>
			<footer class="metadata">
				<time class="published" datetime="2011-11-11" pubdate="">11.11.2011</time>
				<span class="author">Dragan Petos</span>
				<span class="counter">5 komentarjev</span>
			</footer>
			<aside class="entry-share">
				<ul>
					<li class="fb">Social šajze 1</li>
					<li class="twitter">Social šajze 1</li>
					<li class="print"><a title="Natisni" onclick="window.print()" href="#">Print</a></li>
				</ul>
			</aside>
			<section class="entry-summary">
			
				<!-- content -->
				<p><strong>Normally, both your asses would be dead as fucking fried chicken, but you happen to pull this shit while I'm in a transitional period so I don't wanna kill you, I wanna help you. But I can't give you this case, it don't belong to me. Besides, I've already been through too much shit this morning over this case to hand it over to your dumb ass.</strong></p>
				<!-- content -->
			
			</section>
            <!-- ali -->
            <section class="entry-media">

				<video controls="controls" poster="http://sandbox.thewikies.com/vfe-generator/images/big-buck-bunny_poster.jpg" width="640" height="360">
					<source src="http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4" type="video/mp4" />
					<source src="http://clips.vorwaerts-gmbh.de/big_buck_bunny.webm" type="video/webm" />
					<source src="http://clips.vorwaerts-gmbh.de/big_buck_bunny.ogv" type="video/ogg" />
					<object type="application/x-shockwave-flash" data="http://releases.flowplayer.org/swf/flowplayer-3.2.1.swf" width="640" height="360">
						<param name="movie" value="http://releases.flowplayer.org/swf/flowplayer-3.2.1.swf" />
						<param name="allowFullScreen" value="true" />
						<param name="wmode" value="transparent" />
						<param name="flashVars" value="config={'playlist':['http%3A%2F%2Fsandbox.thewikies.com%2Fvfe-generator%2Fimages%2Fbig-buck-bunny_poster.jpg',{'url':'http%3A%2F%2Fclips.vorwaerts-gmbh.de%2Fbig_buck_bunny.mp4','autoPlay':false}]}" />
						<img alt="Big Buck Bunny" src="http://sandbox.thewikies.com/vfe-generator/images/big-buck-bunny_poster.jpg" width="640" height="360" title="No video playback capabilities, please download the video below" />
					</object>
				</video>
				<dl class="download">
					<dt>Download video:</dt> 
					<dd><a href="http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4">MP4 format</a> | 
					<a href="http://clips.vorwaerts-gmbh.de/big_buck_bunny.ogv">Ogg format</a> | 
					<a href="http://clips.vorwaerts-gmbh.de/big_buck_bunny.webm">WebM format</a></dd>
				</dl>
			</section>	
            <!-- ali -->
			<section class="entry-media">
				<figure>
					<a href="" title=""><img src="http://lorempixel.com/640/350/sports/" alt="Slika"/></a>
					<figcaption>Media info</figcaption>
				</figure>
			</section>			
			<section class="entry-content">
				
				<!-- content -->
					<p>Now that we know who you are, I know who I am. I'm not a mistake! It all makes sense! In a comic, you know how you can tell who the arch-villain's going to be? He's the exact opposite of the hero. And most times they're friends, like you and me! I should've known way back when... You know why, David? Because of the kids. They called me Mr Glass.</p>
					<p>Normally, both your asses would be dead as fucking fried chicken, but you happen to pull this shit while I'm in a transitional period so I don't wanna kill you, I wanna help you. But I can't give you this case, it don't belong to me. Besides, I've already been through too much shit this morning over this case to hand it over to your dumb ass.</p>
					<p>Now that we know who you are, I know who I am. I'm not a mistake! It all makes sense! In a comic, you know how you can tell who the arch-villain's going to be? He's the exact opposite of the hero. And most times they're friends, like you and me! I should've known way back when... You know why, David? Because of the kids. They called me Mr Glass.</p>
					<p>Do you see any Teletubbies in here? Do you see a slender plastic tag clipped to my shirt with my name printed on it? Do you see a little Asian child with a blank expression on his face sitting outside on a mechanical helicopter that shakes when you put quarters in it? No? Well, that's what you see at a toy store. And you must think you're in a toy store, because you're here shopping for an infant named Jeb.</p>
					<p>imena classov naj bodo vedno in povsod lowercase, velja za vse bodoce module, ne samo za article.

Struktura classov ostane enaka se pravi: mi imemodula mi1

Classi uporabljeni v articles modulu so napisani po hAtom 0.1 specifikaciji (microformats) http://microformats.org/wiki/hatom-fr  

Se nekaj predlogov:

front:

- friendly url-ji generirani iz article titlov
- social linki
- avtor clanka
- media field v katerega lahko vnasamo slike, video, audio - namesto TeaserImagea (z uporabo video tagov, fallback = flash, primer: article_single_video.html)


backend:
- možnost spreminjanja vrstnega reda fieldov (drupal cck)
- ureditev settingov, grupiranje v logicne skupine z recimo fieldseti
- filtriranje settingov glede na moduleMode (Single / List), glede na to se tudi generira class article(single) oz. articles(list)
- opis settingov, simple hover tooltips o tem kaj zadeva pocne -> title=""


Dodatno:

paginacija, nov html za one.net pager in sicer bi povsod kjer bi potrebovali paginacijo uporabljali enak html (article_list.html)

</p>
				<!-- content -->

			</section>
			<div class="readon">
				<a href="" title="" class="back">&laquo; back</a>
			</div>
		</article>
	</section>	