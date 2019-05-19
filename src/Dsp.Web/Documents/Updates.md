### December 2018

- Modified the website such that new members are actually called "new members."
- Fixed some bugs with paging results, primarily incident reports.

### November 2018

- Reworked user registration:
  - Non-members can now be registered more easily as "Affiliates."
  - Affiliates can only see the meal schedule (for the chef), their account page, and the bug reporting page.
  - Fixed some bugs where affiliates could not see their account page.
- Attempted to fix an issue with uploading an avatar.

---

### October 2018

- Added dietary instructions to member profiles.

---

### September 2018

- Added bug reporting feature.
- Fixed a bug with work order submission.

---

### July-August 2018

- Redesigned the meal schedule functionality to make life easier for house steward (voting removed, hopefully only temporary).
- Trying some caching on service hours/events pages to reduce resource usage and page load times.

---

### June 2018

- Added API endpoint for upcoming sober drivers.
- Added "BA" (Bachelors of Arts) option to add major form.
- The "Add a new class" form on class schedule page will now remember the previously selected semester after you add a class (won't remember if you navigate away from the page though).
- Gave ACB House Manager ability to modify work orders.

---

### April 2018

- Fixed bug with pledge class letter display in profiles.
- Added some details to donations for the Treasurer.
- Fixed bug where you couldn't add new positions.

---

### February 2018

- Fixed a bug where incident reports could not be submitted, and I apparently messed with the emails for incident reports that go to the President and SAA, but I don't remember why.
- Removed laundry statistics, they were unnecessary.
- Removed study hours URL from sidebar. Academic plan must be stable for website support moving forward.
- Fixed a bug where people on iPhone could not fill out the donation form because the iPhone wasn't letting them enter hyphens.
- Moved the "updates" and "External site" links to the "Website" grouping to make more room and in anticipation for a "report an issue" link.
- Fixed a bug where people's profile pictures weren't showing up on the laundry schedule.
- Fixed a bug where Nathan Weidner's existence was preventing the roster CSV download from working - good times.

---

### January 2018

- Implemented internal and external fundraiser, donation, and pledging features.
- Recruitment book is now updated on the website by the DoR entering a URL into one of the Semester fields.

---

### November 2017

- Expanded Incident Reports to include investigation notes visible to Executive Board members.
- Fixed a bug where certain appointments were just not happening.

---

### October 2017

- Migrated website to new host: Amazon Web Services.
- Allowed Affiliates could not be assigned to leadership positions.

---

### September 2017

- Updated Awards.
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


- Recruitment calendar to recruitment page that integrates with dspeboard@gmail.com Google Calendar.
  - Add [RECRUITMENT] to description of calendar event for it to appear there.
- Social calendar to sphinx page that integrates with dspeboard@gmail.com Google Calendar.
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
