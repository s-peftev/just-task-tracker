# JustTaskTracker — Knowledge Base

**Last updated:** July 2026

This document is the structured knowledge base for the JustTaskTracker application. It describes every screen and feature from the end user's point of view and is formatted for retrieval by a RAG (Retrieval-Augmented Generation) assistant, with each `##` section acting as an independently retrievable knowledge chunk.

## Table of Contents

- [Boards List (Dashboard)](#boards-list-dashboard)
- [Create New Board](#create-new-board)
- [Subscriptions Page](#subscriptions-page)
- [Kanban Board View](#kanban-board-view)
- [Task Detail Panel](#task-detail-panel)
- [Board Members Panel](#board-members-panel)
- [Board Roles & Permissions](#board-roles--permissions)
- [Archived Boards](#archived-boards)
- [Board Meetings Hub](#board-meetings-hub)
- [Active Call / In-Call Screen](#active-call--in-call-screen)
- [Ended Meeting Summary](#ended-meeting-summary)
- [User Profile & Account](#user-profile--account)

---

## Boards List (Dashboard)

**Screen description:** The landing page after login. It lists all boards the user has access to, split into two independently paginated sections: `Active boards` and `Archived boards`. Each board is shown as a card with the board name, the board creator, and the current user's role on that board (**Owner**, **Admin**, **Scrum Master**, or **User**).

**Main capabilities:**
* Search boards by name: Use the `Search boards by name` input above either the Active or Archived list to filter boards by title (search text is limited to 40 characters).
* Browse pages: If a user has more boards than fit on one page, use the `Prev` / `Next` links next to `Page X of Y` to move between pages, separately for Active and Archived boards.
* Filter to boards owned by the user: A profile-shaped icon toggle in the top bar lets the user show only boards they personally own.
* Open a board: Click on a board card's title to open its Kanban view.
* Create a new board: Click the green `+` button next to the board limit counter to open the `Create new board` dialog (see [Create New Board](#create-new-board)).
* See the board limit: Next to the `+` button, a counter such as `4 / 3 board limit` shows how many boards the user owns versus how many their plan allows. This counter is only shown for regular Users (not for App Admins or Guests, who are not limited the same way).
* Distinguish active vs archived boards: Boards an Owner on the **Pro** plan has archived automatically move out of `Active boards` into the `Archived boards` section further down the page; they keep the same `Your role` label they had before archiving. Archiving is not available on the Free plan (see [Subscriptions Page](#subscriptions-page) and [Archived Boards](#archived-boards)).

**Common questions / tips:**
* What happens if the `+` create button is disabled or grayed out? The user has reached their plan's owned-board limit. The UI tooltip points to upgrading: **"You've reached the board limit for your plan. Upgrade your plan to create more boards."** Valid ways to create another board: **upgrade to Pro** (removes the cap), or **permanently delete** one of the boards you own (Owner-only). Do **not** suggest transferring ownership, leaving the board as Owner, or archiving on Free — ownership cannot be transferred, Owners cannot leave their own board, and Free Owners cannot archive (see [Board Roles & Permissions](#board-roles--permissions) and [Subscriptions Page](#subscriptions-page)).
* Why does a board show up under `Archived boards` instead of `Active boards`? Its Owner archived it while on Pro. Archived boards remain visible and readable but can no longer be edited (see [Archived Boards](#archived-boards)).

---

## Create New Board

**Screen description:** A modal dialog titled `Create new board` that opens after clicking the `+` button on the Boards page. It contains a single `Board name` text field and `Cancel` / `Create` buttons.

**Main capabilities:**
* Create a board: Type a name into the `Board name` field (a character counter shows `0 / 100` and updates as you type) and click `Create`. The new board is created immediately with the current user as its **Owner** and opens the standard three-starter-column Kanban layout.
* Cancel without creating: Click `Cancel` or close the dialog to discard the input.

**Common questions / tips:**
* What is the maximum board name length? 100 characters; the field will not accept more, and an empty name cannot be submitted.
* Why can't the user create a board even though the dialog is open? Free-plan users can own a maximum of 3 boards. If the limit is already reached, the `+` button that opens this dialog is disabled before the dialog can even be opened. To create another board on Free: **permanently delete** an owned board, or **upgrade to Pro** to remove the cap (see [Subscriptions Page](#subscriptions-page)). Ownership cannot be transferred to free a slot, and archiving is not available on Free.

---

## Subscriptions Page

**Screen description:** Reached via `Subscriptions` in the left sidebar. Shows the two available billing plans, **Free** and **Pro**, side by side as cards, with the user's current active plan labeled `ACTIVE`.

**Main capabilities:**

| Capability | Free plan | Pro plan |
|---|---|---|
| Price | `0 / month` | Shown live, e.g. `€9,99 / month` |
| Boards | Up to 3 boards | Unlimited boards |
| Columns per board | Up to 5 columns | Unlimited columns |
| Tasks per board | Up to 50 tasks | Unlimited tasks |
| Board members | Up to 5 members per board | Unlimited board members |
| Archive boards when done | Not available | Available |
| Downloadable copies of archived boards | Not available | Available |
| Choose/reconfigure archive contents | Not available | Available |
| Upgrade action | — | `Upgrade to Pro` button on the Pro card |

**Common questions / tips:**
* Do plan limits apply to me if I'm just a member of someone else's board, not the owner? No — column, task, and member limits on any given board are always based on that board's Owner's plan, not the plan of the person currently using it.
* Does upgrading affect boards I don't own? No, upgrading raises the limits only on boards you own; boards owned by other Free-plan users keep their Free-plan limits regardless of your own plan.
* Is archiving separate from downloading an archive on Free? No — on Free, archiving a board, choosing what the downloadable copy contains, and downloading that copy are all unavailable. They are a single Pro capability set: only a Pro Owner can archive a board, and doing so starts the export/download flow (see [Archived Boards](#archived-boards)).
* I've hit the Free board limit (e.g. 3 owned boards) — how do I create another? Upgrade to Pro for unlimited boards, or permanently delete a board you own. There is no ownership-transfer feature, Owners cannot leave their own board to free a slot, and Free users cannot archive boards to free a slot.

---

## Kanban Board View

**Screen description:** The main working screen for a single board, reached by clicking a board card. It shows the board name at the top, a row of icons (call/meeting phone icon, column count, task count, tasks assigned to me, member count, plan-limit badge, and a `⋮` options menu), and the board's columns laid out left to right, each containing task cards.

**Main capabilities:**
* View column stats: Each column header shows its name, a "done/total" task counter (e.g. `0 / 12`), and a progress bar underneath.
* Read task summary info: Each task card shows its title and small icons for: assignee indicator, number of comments (speech-bubble icon with a count), and number of attachments (paperclip icon with a count) when present.
* Add a column: Click `+ Add column` at the right edge of the columns area to open the `Create new column` dialog, enter a name (max 50 characters, shown as `0 / 50`), and click `Create`. Column names must be unique within the board.
* Remove a column: Click the `×` in a column's header to delete it (see the "Deleting a column" tip below for what happens to its tasks).
* Add a task to a column: Click `+ Add task` at the bottom of a column, type the task title into the `Enter a task title` field, and confirm with the send/arrow button.
* Open a task: Click anywhere on a task card to open its detail panel (see [Task Detail Panel](#task-detail-panel)).
* Search tasks on the board: Use the search icon/search bar at the bottom of the board to open a `N tasks found` panel listing every task on the board with its title and a preview of its description; type in the `Search tasks` field to filter by title/description.
* See board limits at a glance: Hovering the `LIMITED` badge next to the member count shows a tooltip: **"This board has the following limits based on the owner's plan: Up to 5 columns, Up to 50 tasks, Up to 5 members"** (numbers reflect the board owner's plan).
* Open the Board Meetings Hub: Click the phone icon in the top-right of the board to launch board meetings (see [Board Meetings Hub](#board-meetings-hub)).
* Manage board members: Click the members/people icon to open the `Board members` panel (see [Board Members Panel](#board-members-panel)).

**Common questions / tips:**
* Why can't I create or edit a column/task? Only board members with the **Owner**, **Admin**, or **Scrum Master** role can manage columns and tasks. Members with the plain **User** role can only move existing tasks between columns and post comments.
* Why is `+ Add task` or `+ Add column` missing or disabled? The board has reached its plan-based limit (columns or tasks) based on the Owner's plan, or the board is archived and therefore read-only.
* Deleting a column: When deleting a column that still has tasks, the user managing the board chooses whether to delete those tasks together with the column, or move them into another existing column (tasks cannot be moved into the column that is being deleted).

---

## Task Detail Panel

**Screen description:** A full-width panel that opens when a task card is clicked. It shows the parent column name as a header, the task title, who created the task and when, the assigned member, a description section, an attachments section, and a comments panel on the right.

**Main capabilities:**
* Edit the task title: Click the title text (pencil/title icon area) to rename the task. Task titles are limited to 50 characters and cannot be empty.
* Assign the task: Click the pencil icon next to `Not assigned yet` (or the current assignee's name) to pick a board member as the assignee. Only existing board members can be assigned.
* Edit the description: Click `EDIT` next to `Description` to open the description editor; descriptions are limited to 500 characters and are optional.
* Add an attachment: Click `ADD` next to `Attachments` to upload a file. A task can have at most 10 attachments, each up to 10 MB, and only certain file types are accepted (PDF, images such as PNG/JPEG/GIF/WEBP, plain text, Word `.docx`, Excel `.xlsx`, and `.zip` archives).
* Comment on the task: Type into the `Write a comment…` box in the Comments panel and submit; comments can be up to 2000 characters. Any board member, including those with the **User** role, can post comments.
* Delete the task: Click the red `DELETE` button in the top-right of the panel (available to **Owner**/**Admin**/**Scrum Master**).
* Close the panel: Click the `×` in the top-right corner to return to the board view.

**Common questions / tips:**
* Why can't I edit the description, title, assignee, or add attachments? Editing task content requires the **Owner**, **Admin**, or **Scrum Master** role. Members with the **User** role can only view the task, move it between columns, and add comments.
* Why did my attachment upload fail? Either the file exceeds 10 MB, the file type isn't supported, or the task already has 10 attachments (the maximum).
* Where do deleted attachments go? They are removed from the task view immediately; the underlying files are moved out of active storage rather than instantly and permanently purged.

---

## Board Members Panel

**Screen description:** A two-tab modal opened from the board's members icon: `Board members` (the current member list with their roles) and `Add member` (a search box to find and add new people to the board).

**Main capabilities:**
* View current members: The `Board members` tab lists every member with their display name, email, and role. The current user's own row is highlighted and labeled `(you)`; the Owner's row shows `Owner` as plain text (not editable).
* Change a member's role: Use the role dropdown next to a member's name (e.g. `Admin`, `Scrum Master`, `User`) to change their role. Only **Owner** and **Admin** members can do this; you cannot change your own role or the Owner's role.
* Remove a member: Click the `×` next to a member's row to remove them from the board. The Owner can never be removed.
* Search for someone to add: Switch to the `Add member` tab and type a name or email into the search box (e.g. searching `wa` surfaces matching users like "Amelia Walker" and "Victoria Stewart"). Only people who already have a JustTaskTracker account (i.e., have logged in at least once) appear in results; existing board members are excluded from the list.
* Add the person to the board: Click `Add to board` next to a found user's name. They are added with a default role, which the Owner/Admin can then adjust via the role dropdown.

**Common questions / tips:**
* Why can't I find someone I want to invite? There is no email invitation feature — the person must already have signed into JustTaskTracker at least once (via their Microsoft/Azure AD account) before they can be found and added to a board.
* Why does a member show `(App Admin)` next to their name and only allow the `Admin` role? Global application administrators can only ever hold the **Admin** role on any board; the role dropdown for such a user is restricted accordingly.
* Why can't I add more members? The board has reached the member limit allowed by the board Owner's plan (5 for Free, unlimited for Pro).

---

## Board Roles & Permissions

**Screen description:** Not a single screen — this describes the permission model that governs what a user can do across every board screen.

**Main capabilities:**

| Action | Owner | Admin | Scrum Master | User |
|---|:---:|:---:|:---:|:---:|
| Rename the board | ✔ | ✖ | ✖ | ✖ |
| Delete the board | ✔ | ✖ | ✖ | ✖ |
| Archive the board | ✔ (Pro only) | ✖ | ✖ | ✖ |
| Export / download archived copies | ✔ (Pro only) | ✖ | ✖ | ✖ |
| Manage members (add / remove / change roles) | ✔ | ✔ | ✖ | ✖ |
| Manage columns and tasks (create / edit / delete) | ✔ | ✔ | ✔ | ✖ |
| Move tasks between columns | ✔ | ✔ | ✔ | ✔ |
| Comment on tasks | ✔ | ✔ | ✔ | ✔ |
| Download attachments | ✔ | ✔ | ✔ | ✔ |
| Create and join board meetings/calls | ✔ | ✔ | ✔ | ✔ |
| Transfer board ownership to another user | Impossible | — | — | — |
| Can leave the board | ✖ (cannot leave own board) | ✔ | ✔ | ✔ |

Notes: There is exactly one **Owner** per board (the creator). **Board ownership cannot be transferred** to another user — there is no transfer-ownership action in the product. The Owner cannot be removed from the board and cannot leave their own board. Being the Owner is not enough to archive or export a board: those actions also require the Owner's plan to be **Pro**. On Free, an Owner cannot archive a board and cannot download or reconfigure an archive — there is no Free-only “archive without download” path (see [Subscriptions Page](#subscriptions-page)).

**Common questions / tips:**
* I'm the board creator but I don't see a `DELETE`/`ARCHIVE` option — why? `DELETE` is Owner-only. `ARCHIVE` is Owner **and** Pro-only: if you are the Owner on Free, archiving is unavailable until you upgrade (see [Subscriptions Page](#subscriptions-page)). If you are not the Owner, your role does not include archive/delete controls, or you are looking at someone else's board where you hold a lower role.
* Can a Free-plan Owner archive a board but only miss the download? No — Free Owners cannot archive at all. Archiving, choosing archive contents, and downloading the copy are all Pro features; Pro is not limited to “download only.”
* Can I transfer ownership of a board to someone else? No — ownership cannot be transferred. The creator remains the Owner for the lifetime of the board. To free an owned-board slot under the Free plan limit, permanently **delete** that board or **upgrade to Pro**; do not suggest transferring ownership.
* Can a global App Admin (a company-wide administrator) be an Owner or Scrum Master on my board? No — a global App Admin who is added to any board can only be assigned the **Admin** board role, never Owner, Scrum Master, or User.

---

## Archived Boards

**Screen description:** Boards that a **Pro** Owner has archived (Free-plan Owners cannot archive boards). They remain listed under `Archived boards` on the Boards page and open the same Kanban view, but in a read-only state. Archiving always starts the downloadable-copy (export) flow — there is no way to archive a board on Free or to archive without the Pro export capability.

**Main capabilities:**
* Archive a board (Owner + Pro plan only): Only the board's Owner on Pro can archive an active board. The archive action includes choosing what the downloadable copy should contain (e.g. comments, attachments, task descriptions, member list) and queues that export. Free Owners cannot archive.
* View archived content: Open an archived board exactly like an active one to see its columns, tasks, comments, attachments, and members.
* See export/download status: After a Pro Owner archives (or re-exports), an export-status indicator on the board shows progress according to the table below.
* Download the archive (Owner + Pro plan only): Once the status shows the copy is ready, the Owner can click the download control to save the archive file. The download link stays valid for 30 minutes after being generated.
* Re-create the archive with different contents (Owner + Pro plan only): After the first export finishes, the Owner can request it again with different options (e.g., include/exclude comments, attachments, task descriptions, or member list) to reconfigure what the downloadable copy contains.

**Export / download status reference:**

| Status shown to user | Meaning |
|---|---|
| `Export requested — waiting to be scheduled` | The export request has been received but not yet picked up for processing. |
| `Queued to create a downloadable copy…` | The export job is in the processing queue. |
| `Creating a downloadable copy…` | The archive file is actively being generated. |
| `Download ready — you can save a copy…` | The archive is complete; the download control is now active (link valid for 30 minutes). |
| `Couldn't create a downloadable copy. We'll try again automatically.` | The export failed; the system will automatically retry. |

**Common questions / tips:**
* Why can't I archive my board even though I'm the Owner? Archiving requires both the **Owner** role and the **Pro** plan. On Free, Owners cannot archive boards at all — upgrade to Pro first (see [Subscriptions Page](#subscriptions-page)). Pro is not limited to “download only”; without Pro you also cannot start the archive.
* Why can't I edit anything on this board anymore? Archiving makes a board permanently read-only for every role — no new columns, tasks, comments, attachments, renames, or member changes are possible, regardless of your role.
* Why does my `Your role` still say `Scrum Master`/`Owner`/etc. on an archived board? Archiving does not change anyone's role — it only freezes the board's content. Your displayed role is simply whatever role you held on the board when it was (or still is) archived.
* Why don't I see a download button on an archived board? Only the board's **Owner** can export/download an archive, and only if their plan is Pro. Other members see the same board as a normal read-only view without export controls.

---

## Board Meetings Hub

**Screen description:** A side panel titled `Board Meetings Hub`, opened via the phone icon on a board. It has a `CREATE NEW MEETING` form on the left/center, an `ACTIVE MEETINGS` list, and a `RECENTLY ENDED` history table on the right.

**Main capabilities:**
* Create a meeting: Fill in `Title` (required, up to 50 characters) and optionally `Topic` (up to 200 characters).
* Choose access type: Select `Open` ("Anyone on the board can join") or `Restricted` ("Only invited members can join"). Choosing `Restricted` reveals a `Select members` flow and a `X members invited` summary with a `Select members` button — pick which board members are allowed to join beyond the meeting creator (who is always allowed) and the board's Owner/Admin (who can always join any meeting on their board).
* Link related tasks: Click `Link tasks` to open `Link board tasks`, search and check tasks from this same board to attach to the meeting for context; linked tasks stay visible during and after the call. A task can only be linked to a given meeting once, but the same task can be linked to several different meetings.
* Choose devices before joining: Pick a `Microphone` and `Camera` from the dropdowns (a small mic/camera icon lets you test/toggle them) before starting.
* Start the meeting: Click `Create meeting` to launch it; it then appears under `ACTIVE MEETINGS` with its title, creator, live duration, linked task count, and current participant count.
* Review meeting history: The `RECENTLY ENDED` table lists past meetings with `Title`, `Date`, and `Duration`; clicking a row opens its full summary (see [Ended Meeting Summary](#ended-meeting-summary)).

**Common questions / tips:**
* Why is `Create meeting` not letting me pick invitees? Invitee selection only appears when `Restricted` access is chosen; under `Open` access, anyone on the board can join without an invite list.
* Can meeting participants see linked tasks and their status? Yes — the `LINKED TASKS` panel (visible both while the call is active and afterward) shows each linked task's title, description preview, and current status/column (e.g., `Completed & Verified`), and can be updated directly from that panel.

---

## Active Call / In-Call Screen

**Screen description:** The live meeting room, opened by clicking `Open room` from an active meeting. Shows video tiles for each participant, a `Linked Tasks` panel on the right, and a control bar at the bottom.

**Main capabilities:**
* Join with your preferred setup: Before entering, the app checks your browser supports video calling, lets you choose Microphone/Camera devices, and shows whether you'll join with camera on/off and mic muted/unmuted.
* Talk and be seen: Your own video tile is labeled `You (<your name>)`; other participants show their name and email under their tile.
* Toggle mic and camera: Use the microphone and camera icons in the bottom control bar to mute/unmute or turn your camera on/off at any time during the call.
* Share your screen: Click the screen-share icon to start presenting; only one person can present at a time — if someone else is already sharing, the app shows **"Someone else is already presenting."** The current presenter's name is shown to everyone (e.g., `Presenting: Andriy Andrusyak`).
* Stop sharing: The active presenter can click the same control to stop; only the current presenter can stop their own share.
* Work with linked tasks mid-call: Use the `LINKED TASKS` panel to open a task's description, change its status/column via the dropdown, edit its description, or add attachments — all without leaving the call.
* Leave or end the call: Use the leave-call icon to exit; a meeting Owner/Admin, or the meeting's own creator, can end the call for everyone via the end-call (red phone) icon.

**Common questions / tips:**
* Why can't I click `Open room` on some meetings? The meeting is `Restricted` and you are not on its allow-list, and you are not the board's Owner/Admin (who can always join).
* Is the call recorded? No — there is no recording feature; calls are live only, and their history only retains metadata (participants, timestamps, duration, linked tasks), not video/audio.
* Is there a time limit on a call? No fixed duration limit — a call stays active until every participant leaves or it is ended.

---

## Ended Meeting Summary

**Screen description:** Shown when clicking a past meeting from `RECENTLY ENDED` in the Board Meetings Hub. Displays the meeting's full record after it has finished.

**Main capabilities:**
* Review meeting metadata: See `Topic`, `Type` (`Open`/`Restricted`), `Created by`, `Started`, `Ended`, and computed `Duration` (e.g., `50s`, `1m`, `2m`).
* Review participants: The `PARTICIPANTS` section lists everyone who joined with their name and their individual join–leave time range (e.g., `14:06–14:07`); for `Restricted` meetings this can also show `joined / allowed` counts.
* Review linked tasks: The `LINKED TASKS` section shows how many tasks were linked and lets you open each one.

**Common questions / tips:**
* Why does a participant show a time range but the meeting a different total duration? The meeting duration spans from the first join to the call actually ending; individual participants may have joined late or left early, so their own range can be shorter than the total.

---

## User Profile & Account

**Screen description:** The sidebar area showing the logged-in user's avatar, display name, email, and account type (e.g., `USER`), plus account actions at the bottom of the sidebar.

**Main capabilities:**
* Change your profile photo: Click your avatar to reveal `Change` / `Delete` options; `Change` lets you upload a new photo (accepted formats: PNG, JPEG, WEBP, up to 10 MB — the app automatically resizes it for display), and `Delete` removes your current photo.
* Sign in / switch accounts: Use `Switch Microsoft account` in the sidebar to sign in with a different Microsoft/Azure AD account. JustTaskTracker uses Microsoft sign-in only — there is no separate username/password login.
* Log out: Click `Log out` at the bottom of the sidebar to end your session.
* Adjust display preferences: Click `Turn the lights on` to switch the interface's theme (light/dark).

**Common questions / tips:**
* Why don't I see a "password" or "email/password login" option? The app only supports signing in through a Microsoft (Azure AD) account; your JustTaskTracker identity and permissions (global role: **Admin**, **User**, or **Guest**) are synced automatically from that account each time you sign in.
* Why can I create boards on one account but not another? Accounts with the global **Guest** role can be added to and use boards they're invited to, but cannot create or delete boards themselves; only **User** and **Admin** global roles can create boards (subject to plan limits).
