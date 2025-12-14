                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                module aidchain::impact_nft {
    use std::string::{Self, String};
    use sui::url::{Self, Url};
    use sui::event;

    // ============================================
    // CONSTANTS - Tier Thresholds (in MIST, 1 SUI = 1_000_000_000 MIST)
    // ============================================
    const TIER_BRONZE_MIN: u64 = 100_000_000;      // 0.1 SUI
    const TIER_SILVER_MIN: u64 = 1_000_000_000;    // 1 SUI
    const TIER_GOLD_MIN: u64 = 10_000_000_000;     // 10 SUI
    const TIER_DIAMOND_MIN: u64 = 100_000_000_000; // 100 SUI

    // Tier codes
    const TIER_BRONZE: u8 = 1;
    const TIER_SILVER: u8 = 2;
    const TIER_GOLD: u8 = 3;
    const TIER_DIAMOND: u8 = 4;

    // Error codes
    const E_AMOUNT_TOO_LOW: u64 = 1;

    // ============================================
    // NFT STRUCT
    // ============================================

    /// Proof of Impact NFT - Soul-Bound Token (non-transferable)
    public struct ImpactNFT has key, store {
        id: UID,
        /// NFT name
        name: String,
        /// Description of the donation
        description: String,
        /// Image URL (IPFS or generated)
        image_url: Url,
        /// Donation amount in MIST
        donation_amount: u64,
        /// Recipient location
        recipient_location: String,
        /// Recipient name (anonymized)
        recipient_name: String,
        /// Tier level (1=Bronze, 2=Silver, 3=Gold, 4=Diamond)
        tier: u8,
        /// Tier name as string
        tier_name: String,
        /// Donation timestamp (epoch)
        donated_at_epoch: u64,
        /// Original donor address
        donor: address,
        /// Unique impact ID (sequential)
        impact_id: u64,
    }

    /// Global counter for impact IDs
    public struct ImpactCounter has key {
        id: UID,
        count: u64,
    }

    // ============================================
    // EVENTS
    // ============================================

    public struct ImpactNFTMinted has copy, drop {
        nft_id: ID,
        impact_id: u64,
        donor: address,
        amount: u64,
        tier: u8,
        tier_name: String,
        recipient_location: String,
        epoch: u64,
    }

    // ============================================
    // INIT
    // ============================================

    fun init(ctx: &mut TxContext) {
        let counter = ImpactCounter {
            id: object::new(ctx),
            count: 0,
        };
        transfer::share_object(counter);
    }

    // ============================================
    // PUBLIC FUNCTIONS
    // ============================================

    /// Mint a Proof of Impact NFT for a donor
    public entry fun mint_impact_nft(
        counter: &mut ImpactCounter,
        donation_amount: u64,
        recipient_location: vector<u8>,
        recipient_name: vector<u8>,
        ctx: &mut TxContext
    ) {
        // Validate minimum donation
        assert!(donation_amount >= TIER_BRONZE_MIN, E_AMOUNT_TOO_LOW);

        let sender = tx_context::sender(ctx);
        let current_epoch = tx_context::epoch(ctx);

        // Determine tier
        let (tier, tier_name) = get_tier(donation_amount);

        // Increment counter
        counter.count = counter.count + 1;
        let impact_id = counter.count;

        // Create NFT name
        let name = create_nft_name(impact_id, tier_name);

        // Create description
        let description = create_description(donation_amount, string::utf8(recipient_location));

        // Generate image URL based on tier
        let image_url = get_tier_image_url(tier);

        // Create NFT
        let nft = ImpactNFT {
            id: object::new(ctx),
            name,
            description,
            image_url,
            donation_amount,
            recipient_location: string::utf8(recipient_location),
            recipient_name: string::utf8(recipient_name),
            tier,
            tier_name,
            donated_at_epoch: current_epoch,
            donor: sender,
            impact_id,
        };

        // Emit event
        event::emit(ImpactNFTMinted {
            nft_id: object::id(&nft),
            impact_id,
            donor: sender,
            amount: donation_amount,
            tier,
            tier_name,
            recipient_location: string::utf8(recipient_location),
            epoch: current_epoch,
        });

        // Transfer to donor
        transfer::public_transfer(nft, sender);
    }

    // ============================================
    // HELPER FUNCTIONS
    // ============================================

    /// Determine tier based on donation amount
    fun get_tier(amount: u64): (u8, String) {
        if (amount >= TIER_DIAMOND_MIN) {
            (TIER_DIAMOND, string::utf8(b"Diamond"))
        } else if (amount >= TIER_GOLD_MIN) {
            (TIER_GOLD, string::utf8(b"Gold"))
        } else if (amount >= TIER_SILVER_MIN) {
            (TIER_SILVER, string::utf8(b"Silver"))
        } else {
            (TIER_BRONZE, string::utf8(b"Bronze"))
        }
    }

    /// Generate image URL based on tier
    fun get_tier_image_url(tier: u8): Url {
        // Using placeholder images - can be replaced with actual NFT art
        let base_url = if (tier == TIER_DIAMOND) {
            b"https://raw.githubusercontent.com/AidChainSui/assets/main/diamond.png"
        } else if (tier == TIER_GOLD) {
            b"https://raw.githubusercontent.com/AidChainSui/assets/main/gold.png"
        } else if (tier == TIER_SILVER) {
            b"https://raw.githubusercontent.com/AidChainSui/assets/main/silver.png"
        } else {
            b"https://raw.githubusercontent.com/AidChainSui/assets/main/bronze.png"
        };
        url::new_unsafe_from_bytes(base_url)
    }

    /// Create NFT name
    fun create_nft_name(impact_id: u64, tier_name: String): String {
        let mut name = string::utf8(b"AidChain Impact #");
        string::append(&mut name, u64_to_string(impact_id));
        string::append(&mut name, string::utf8(b" ["));
        string::append(&mut name, tier_name);
        string::append(&mut name, string::utf8(b"]"));
        name
    }

    /// Create description
    fun create_description(amount: u64, location: String): String {
        let mut desc = string::utf8(b"This NFT certifies a donation of ");
        string::append(&mut desc, format_sui_amount(amount));
        string::append(&mut desc, string::utf8(b" SUI to support disaster relief efforts in "));
        string::append(&mut desc, location);
        string::append(&mut desc, string::utf8(b". Thank you for making a difference!"));
        desc
    }

    /// Convert u64 to String
    fun u64_to_string(value: u64): String {
        if (value == 0) {
            return string::utf8(b"0")
        };

        let mut buffer = vector::empty<u8>();
        let mut n = value;

        while (n > 0) {
            let digit = ((n % 10) as u8) + 48; // ASCII '0' = 48
            vector::push_back(&mut buffer, digit);
            n = n / 10;
        };

        vector::reverse(&mut buffer);
        string::utf8(buffer)
    }

    /// Format SUI amount (convert MIST to SUI with decimals)
    fun format_sui_amount(mist: u64): String {
        let sui = mist / 1_000_000_000;
        let decimals = (mist % 1_000_000_000) / 100_000_000; // First decimal

        let mut result = u64_to_string(sui);
        if (decimals > 0) {
            string::append(&mut result, string::utf8(b"."));
            string::append(&mut result, u64_to_string(decimals));
        };
        result
    }

    // ============================================
    // VIEW FUNCTIONS
    // ============================================

    /// Get total impact NFTs minted
    public fun get_total_impacts(counter: &ImpactCounter): u64 {
        counter.count
    }

    /// Get NFT details
    public fun get_nft_info(nft: &ImpactNFT): (String, u64, u8, String, u64) {
        (nft.name, nft.donation_amount, nft.tier, nft.recipient_location, nft.impact_id)
    }

    /// Check if address is original donor (for soul-bound verification)
    public fun is_original_donor(nft: &ImpactNFT, addr: address): bool {
        nft.donor == addr
    }

    // ============================================
    // TIER INFO (for frontend)
    // ============================================

    /// Get tier thresholds
    public fun get_tier_thresholds(): (u64, u64, u64, u64) {
        (TIER_BRONZE_MIN, TIER_SILVER_MIN, TIER_GOLD_MIN, TIER_DIAMOND_MIN)
    }
}
