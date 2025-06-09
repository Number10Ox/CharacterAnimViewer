
JARS AI Takehome Assignment
Character Animation Viewer

I. Brief description of your implementation

I decided to implement a solution like I would in an actual game project:

a. Data-driven

The animation categories and list of animations per category is specified inx
Scriptable object tuning data. 

I didn't want to set up a Mech Anim animation controller to support a fixed set
of animations, although that would have been quicker an easier. I went with a
solution that we used on Crypto Unicorns for the unicorns and extracted the
animations and played them on the base model used in all of the animations.

With more time, I would have looked into using Unity Playables instead. 

b. UI Toolkit

I usually use standard Unity UI everyday, I deciced to use UI Toolkit for the
UI. We started using UI Toolkit for tools on my last project, and I would
look into using it more in the future both for tools and in game.

I am rendering the character into a render texture bound to a UI toolkit
visual element. This is less efficient that rendering to the main camera,
but I think this is a tool and I think you need to do it this way for
UI Toolkit. I would look into this further.

c. Addressables asset loading

I decided to use Addressables for loading the model and and animation assets.
I'm used to using custom CMS solutions with loading of asset bundles. 
There was no way I was going to use Resources load.

d. Preloading assets before showing dialog

I'd used to having a Pop up and preloading system - loading assets before 
showing UI. I opted to go with the popup being persistent and load all 
assets before the first time it was shown.

II. Improvements I'd make given time

1. Add ability for user to play animation in a loop
2. Add camera control
3. Add some loading UI before pop up is displayed
4. Improve the UI - the UI Toolkit UI implemented has no frills visuals for
selection and highlighting, etc. I'd look into how to make UI Toolkit UI
look shinier. I'd also probably breakup the USS so cards had their own and
define a struct for the names instead of all the constants in the popup class.
5. Look into using Unity Playables instead of of the solution with a simple
AnimationController and an AnimationOverrideController






