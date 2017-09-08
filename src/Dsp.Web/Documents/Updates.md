### September 2017

- Updated Awards
- Changed the titles of the "Sobers and Socials" tabs on the sphinx page.
- Fixed profile picture display bug.

---

### August 2017

- Fixed bug with semester creation.

---

### July 2017

- Changed the way the contacts page looks and added profile picture notifications.

---

### March 2017

- Removed ability to cencel sober signups; tweaked layout; added button to single-click copy phone number (only works in newer browsers).
- The social tab on the sphinx page now shows the number of upcoming social events since it's not selected by default.
- Chapter `about` page was given some images and the layout was modified.
- External page links that refer visitors to the contacts page will now take them directly to the member on the contacts they need to contact 
  and even highlight them.

---

### February 2017

- Four or more meal plate signups in the sphinx feed within a given day will now be collapsed into a single item displaying the total.
  The time component represents the time since the most recent signup.
- Late plates in the feed can now be seen by everyone instead of just the house steward and administrators.
- Four or more laundry signups in the sphinx feed within a given day will now be collapsed into a single item displaying the total.
  The time component represents the time since the most recent signup.

---

### January 2017


- Recruitment calendar to recruitment page that integrates with ***REMOVED*** Google Calendar.
  - Add [RECRUITMENT] to description of calendar event for it to appear there.
- Social calendar to sphinx page that integrates with ***REMOVED*** Google Calendar.
  - Add [SOCIAL] to description of calendar event for it to appear there
- Caching for certain pages in attempt to reduce data usage and page load time.
  - **As a result, updates to sphinx, roster, and external pages will be delayed from 1 minute to an hour depending on the page.**
- Chapter status box redesigned and made visible to all members on sphinx page.
- New *Updates* page for public record of changes @ `deltasig.de/updates` (aka this one).
- Sphinx notifications were redesigned appearance-wise and moved above sober signups to be more prominent.
- Laundry and sober schedules were moved out from under sphinx in URL because there was no meaningful reason for that.
  - `deltasig.de/sphinx/home` -> `deltasig.de/sphinx`
  - `deltasig.de/sphinx/laundry/schedule` -> `deltasig.de/laundry/schedule`
  - `deltasig.de/sphinx/sobers/sechedule` - > `deltasig.de/sobers/schedule`
- A lot of refactoring under the hood, such as making the sphinx feed use the API and `mustache.js` (this for me, not you).
- Fixed display problems on sober types pages - been ignoring it for a while.
- Resolved mimtype browser warnings - also been there for a while.
- Removed BroQuest.

---

### August 2016

- Updated awards on external *About* page.
- Fixed Position editing bug.
- Fixed time display in error logs.
- Fixed bug preventing secretary from editing accounts.
- Numerous BroQuest bug fixes.

---

### July 2016

- Added BroQuest
- Fixed up *Appointments* page.
- Fixed bug with new account registration.
- Fixed bug with validation of naming on uploaded class files.

---

### May 2016

- Expanded validation of naming on uploaded class files.
- Added class file upload instructions and change limitations.
- Altered source code project structure to separate concerns.
- Fixed bug with disenroling from class when duplicate is created - frikkin weird.
- Removed print icon from external pages.

---

### February 2016

- Fixed display ordering of scholarships.
- Added color legend to study report page.
- Initial setup for eventual chore implementation - *sighs*.

---

### January 2016

- Updated sphinx sober schedule display algorithm to show next seven days in order to handle entire weeks of sober people.
- Made many pages look decent when printing them in the browser.
- Refactored kitchen, phone number, address, and recruitment pages.
- Added flickr link to external footer.
- Fixed service index display bug.
- Map on home page is static image instead of pulling from google maps.
- Added *About* page.
- Position permissions and contact page were moved to rely on semester transition date instead of semester end.
- Fixed avatar display bug, but I would eventually delete everyone's profile pic months later...
